using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wichteln.Shared;

namespace Wichteln.Tests;

public sealed class FakeClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;
    public override DateTimeOffset GetUtcNow() => Now;
}

public sealed class ApiTests : IDisposable
{
    private static readonly DateTimeOffset Unlock = new(2026, 11, 20, 0, 0, 0, TimeSpan.FromHours(1));
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"wichteln-test-{Guid.NewGuid():N}.db");
    private readonly FakeClock _clock = new(Unlock.AddDays(-5));
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("Db:Provider", "sqlite");
            b.UseSetting("ConnectionStrings:Wichteln", $"Data Source={_dbPath}");
            b.UseSetting("Wichteln:AdminKey", "test-key");
            b.UseSetting("Wichteln:PublicAppUrl", "https://wichteln.test");
            b.ConfigureServices(s => { s.RemoveAll<TimeProvider>(); s.AddSingleton<TimeProvider>(_clock); });
        });
    }

    public void Dispose()
    {
        _factory.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch { }
    }

    private sealed record LinkRow(string Name, string Link, bool Revealed, int Wishes);

    private async Task<Dictionary<string, HttpClient>> ClientsAsync()
    {
        var admin = _factory.CreateClient();
        admin.DefaultRequestHeaders.Add("X-Admin-Key", "test-key");
        var links = await admin.GetFromJsonAsync<List<LinkRow>>("/admin/links");
        Assert.Equal(["Justin", "Luana", "Lydia", "Angelika", "Denis"], links!.Select(l => l.Name));
        return links.ToDictionary(l => l.Name, l =>
        {
            var c = _factory.CreateClient();
            var token = l.Link.Split("login=")[1];
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return c;
        });
    }

    [Fact]
    public async Task Unknown_link_is_rejected_and_admin_needs_key()
    {
        var c = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/me")).StatusCode);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "nope");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync("/admin/links")).StatusCode);
    }

    [Fact]
    public async Task Wishlist_editable_before_unlock_and_frozen_after()
    {
        var c = (await ClientsAsync())["Lydia"];
        var add = await c.PostAsJsonAsync("/api/wishes", new WishInput("Kuschelsocken", "https://amazon.de/socken"));
        Assert.Equal(HttpStatusCode.OK, add.StatusCode);
        var wish = await add.Content.ReadFromJsonAsync<WishDto>();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/wishes", new WishInput("", null))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/wishes", new WishInput("x", "javascript:alert(1)"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await c.PutAsJsonAsync($"/api/wishes/{wish!.Id}", new WishInput("Wollsocken", null))).StatusCode);

        // Someone else cannot change Lydia's wish.
        var other = (await ClientsAsync())["Denis"];
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/wishes/{wish.Id}")).StatusCode);

        var me = await c.GetFromJsonAsync<MeDto>("/api/me");
        Assert.False(me!.IsUnlocked);
        Assert.Equal("Wollsocken", Assert.Single(me.Wishes).Title);
        Assert.Equal(4, me.Others.Count);

        // Reveal is locked before the date.
        Assert.Equal(HttpStatusCode.Forbidden, (await c.PostAsync("/api/reveal", null)).StatusCode);

        _clock.Now = Unlock;
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/wishes", new WishInput("Zu spät", null))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.DeleteAsync($"/api/wishes/{wish.Id}")).StatusCode);
    }

    [Fact]
    public async Task Reveal_gives_everyone_a_valid_unique_wichtel_with_their_list()
    {
        var clients = await ClientsAsync();
        await clients["Luana"].PostAsJsonAsync("/api/wishes", new WishInput("Matcha-Set", null));
        _clock.Now = Unlock.AddMinutes(1);

        // Everyone reveals at the same time.
        var results = await Task.WhenAll(clients.Select(async kv =>
        {
            var res = await kv.Value.PostAsync("/api/reveal", null);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            return (Giver: kv.Key, Wichtel: (await res.Content.ReadFromJsonAsync<WichtelDto>())!);
        }));

        var map = results.ToDictionary(r => r.Giver, r => r.Wichtel.Person.Name);
        Assert.Equal(5, map.Values.Distinct().Count());
        Assert.All(map, kv => Assert.NotEqual(kv.Key, kv.Value));
        Assert.NotEqual("Luana", map["Justin"]); Assert.NotEqual("Justin", map["Luana"]);
        Assert.NotEqual("Denis", map["Angelika"]); Assert.NotEqual("Angelika", map["Denis"]);

        var luanasSanta = results.Single(r => r.Wichtel.Person.Name == "Luana");
        Assert.Equal("Matcha-Set", Assert.Single(luanasSanta.Wichtel.Wishes).Title);

        // Revealing again returns the same person, and /me remembers it.
        foreach (var (name, c) in clients)
        {
            var again = await (await c.PostAsync("/api/reveal", null)).Content.ReadFromJsonAsync<WichtelDto>();
            Assert.Equal(map[name], again!.Person.Name);
            Assert.True((await c.GetFromJsonAsync<MeDto>("/api/me"))!.HasRevealed);
            Assert.Equal(map[name], (await c.GetFromJsonAsync<WichtelDto>("/api/wichtel"))!.Person.Name);
        }
    }

    [Fact]
    public async Task Wichtel_is_hidden_until_you_reveal()
    {
        var clients = await ClientsAsync();
        _clock.Now = Unlock.AddHours(1);
        await clients["Denis"].PostAsync("/api/reveal", null);
        Assert.Equal(HttpStatusCode.NotFound, (await clients["Justin"].GetAsync("/api/wichtel")).StatusCode);
    }
}
