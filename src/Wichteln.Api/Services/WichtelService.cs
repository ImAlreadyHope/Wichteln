using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wichteln.Api.Data;
using Wichteln.Shared;

namespace Wichteln.Api.Services;

public sealed class WichtelService(WichtelDb db, IOptions<WichtelOptions> options, TimeProvider clock)
{
    // One API instance runs the draw; this keeps two simultaneous first reveals from racing.
    private static readonly SemaphoreSlim DrawLock = new(1, 1);
    private readonly WichtelOptions _o = options.Value;

    public bool IsUnlocked => clock.GetUtcNow() >= _o.UnlockAt;

    public Task<Participant?> FindByTokenAsync(string? token) =>
        string.IsNullOrWhiteSpace(token) || token.Length > 64
            ? Task.FromResult<Participant?>(null)
            : db.Participants.FirstOrDefaultAsync(p => p.Token == token);

    public async Task<MeDto> GetMeAsync(Participant me)
    {
        var wishes = await WishesOf(me.Id);
        var others = await db.Participants.Where(p => p.Id != me.Id).OrderBy(p => p.Id)
            .Select(p => new PersonDto(p.Id, p.Name)).ToListAsync();
        return new MeDto(me.Id, me.Name, _o.UnlockAt, clock.GetUtcNow(), IsUnlocked, me.RevealedAt != null, wishes, others);
    }

    public async Task<List<WishDto>> WishesOf(int participantId) =>
        await db.Wishes.Where(w => w.ParticipantId == participantId).OrderBy(w => w.Id)
            .Select(w => new WishDto(w.Id, w.Title, w.Url)).ToListAsync();

    public async Task<(WishDto? Wish, string? Error)> AddWishAsync(Participant me, WishInput input)
    {
        if (IsUnlocked) return (null, "Die Wunschlisten sind seit der Ziehung fest.");
        if (WishRules.Validate(input) is { } err) return (null, err);
        if (await db.Wishes.CountAsync(w => w.ParticipantId == me.Id) >= WishRules.MaxWishes)
            return (null, $"Höchstens {WishRules.MaxWishes} Wünsche.");
        var wish = new Wish { ParticipantId = me.Id, Title = input.Title.Trim(), Url = Clean(input.Url), CreatedAt = clock.GetUtcNow() };
        db.Wishes.Add(wish);
        await db.SaveChangesAsync();
        return (new WishDto(wish.Id, wish.Title, wish.Url), null);
    }

    public async Task<(WishDto? Wish, string? Error, bool NotFound)> UpdateWishAsync(Participant me, int id, WishInput input)
    {
        if (IsUnlocked) return (null, "Die Wunschlisten sind seit der Ziehung fest.", false);
        var wish = await db.Wishes.FirstOrDefaultAsync(w => w.Id == id && w.ParticipantId == me.Id);
        if (wish is null) return (null, null, true);
        if (WishRules.Validate(input) is { } err) return (null, err, false);
        wish.Title = input.Title.Trim();
        wish.Url = Clean(input.Url);
        await db.SaveChangesAsync();
        return (new WishDto(wish.Id, wish.Title, wish.Url), null, false);
    }

    public async Task<(string? Error, bool NotFound)> DeleteWishAsync(Participant me, int id)
    {
        if (IsUnlocked) return ("Die Wunschlisten sind seit der Ziehung fest.", false);
        var wish = await db.Wishes.FirstOrDefaultAsync(w => w.Id == id && w.ParticipantId == me.Id);
        if (wish is null) return (null, true);
        db.Wishes.Remove(wish);
        await db.SaveChangesAsync();
        return (null, false);
    }

    /// <summary>Marks the person as revealed and returns their Wichtel. Null before the unlock date.</summary>
    public async Task<WichtelDto?> RevealAsync(Participant me)
    {
        if (!IsUnlocked) return null;
        await EnsureDrawAsync();
        if (me.RevealedAt is null)
        {
            me.RevealedAt = clock.GetUtcNow();
            await db.SaveChangesAsync();
        }
        return await GetWichtelAsync(me);
    }

    /// <summary>The Wichtel of someone who has already revealed; null otherwise.</summary>
    public async Task<WichtelDto?> GetWichtelAsync(Participant me)
    {
        if (me.RevealedAt is null) return null;
        var a = await db.Assignments.FirstOrDefaultAsync(x => x.GiverId == me.Id);
        if (a is null) return null;
        var r = await db.Participants.FirstAsync(p => p.Id == a.ReceiverId);
        return new WichtelDto(new PersonDto(r.Id, r.Name), await WishesOf(r.Id));
    }

    public async Task EnsureDrawAsync()
    {
        if (await db.Assignments.AnyAsync()) return;
        await DrawLock.WaitAsync();
        try
        {
            if (await db.Assignments.AnyAsync()) return;
            var people = await db.Participants.OrderBy(p => p.Id).ToListAsync();
            var byName = people.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
            var forbidden = _o.ForbiddenPairs
                .Where(pair => pair.Length == 2 && byName.ContainsKey(pair[0]) && byName.ContainsKey(pair[1]))
                .Select(pair => (byName[pair[0]], byName[pair[1]]));
            var map = DrawService.Generate(people.Select(p => p.Id).ToList(), forbidden, Random.Shared);
            db.Assignments.AddRange(map.Select(kv => new Assignment { GiverId = kv.Key, ReceiverId = kv.Value }));
            await db.SaveChangesAsync();
        }
        finally { DrawLock.Release(); }
    }

    /// <summary>Creates the configured people with fresh link tokens. Existing people keep their links.</summary>
    public async Task SeedAsync()
    {
        var existing = await db.Participants.Select(p => p.Name).ToListAsync();
        foreach (var name in _o.People.Where(n => !existing.Contains(n, StringComparer.OrdinalIgnoreCase)))
            db.Participants.Add(new Participant { Name = name, Token = NewToken() });
        await db.SaveChangesAsync();
    }

    public async Task<object> LinksAsync() =>
        (await db.Participants.OrderBy(p => p.Id).ToListAsync()).Select(p => new
        {
            p.Name,
            Link = $"{_o.PublicAppUrl.TrimEnd('/')}/?login={p.Token}",
            Revealed = p.RevealedAt != null,
            Wishes = db.Wishes.Count(w => w.ParticipantId == p.Id),
        }).ToList();

    public async Task ResetDrawAsync()
    {
        db.Assignments.RemoveRange(db.Assignments);
        foreach (var p in db.Participants) p.RevealedAt = null;
        await db.SaveChangesAsync();
    }

    public async Task<bool> NewLinkAsync(string name)
    {
        var p = await db.Participants.FirstOrDefaultAsync(x => x.Name == name);
        if (p is null) return false;
        p.Token = NewToken();
        await db.SaveChangesAsync();
        return true;
    }

    private static string? Clean(string? url) => string.IsNullOrWhiteSpace(url) ? null : url.Trim();

    private static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
