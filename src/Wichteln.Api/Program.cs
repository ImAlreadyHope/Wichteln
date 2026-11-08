using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wichteln.Api.Data;
using Wichteln.Api.Services;
using Wichteln.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WichtelOptions>(builder.Configuration.GetSection("Wichteln"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<WichtelService>();

var provider = builder.Configuration["Db:Provider"] ?? "mysql";
var conn = builder.Configuration.GetConnectionString("Wichteln")
    ?? throw new InvalidOperationException("ConnectionStrings:Wichteln fehlt.");
builder.Services.AddDbContext<WichtelDb>(o =>
{
    if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase)) o.UseSqlite(conn);
    else
    {
        var version = builder.Configuration["Db:ServerVersion"];
        o.UseMySql(conn, string.IsNullOrWhiteSpace(version) ? ServerVersion.AutoDetect(conn) : ServerVersion.Parse(version),
            m => m.EnableRetryOnFailure(3));
    }
});

var origins = builder.Configuration.GetSection("Wichteln:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear(); // Caddy runs in the same Docker network
    o.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors();
app.UseRateLimiter();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WichtelDb>();
    await db.Database.EnsureCreatedAsync();
    await scope.ServiceProvider.GetRequiredService<WichtelService>().SeedAsync();
}

app.MapGet("/health", () => Results.Ok(new { ok = true }));

var api = app.MapGroup("/api").AddEndpointFilter(async (ctx, next) =>
{
    // Personal-link login: the token travels as a bearer token.
    var http = ctx.HttpContext;
    var header = http.Request.Headers.Authorization.ToString();
    var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
    var me = await http.RequestServices.GetRequiredService<WichtelService>().FindByTokenAsync(token);
    if (me is null) return Results.Problem("Dieser Link ist ungültig.", statusCode: StatusCodes.Status401Unauthorized);
    http.Items["me"] = me;
    return await next(ctx);
});

static Participant Me(HttpContext http) => (Participant)http.Items["me"]!;

api.MapGet("/me", async (HttpContext http, WichtelService s) => Results.Ok(await s.GetMeAsync(Me(http))));

api.MapPost("/wishes", async (HttpContext http, WishInput input, WichtelService s) =>
{
    var (wish, error) = await s.AddWishAsync(Me(http), input);
    return error is null ? Results.Ok(wish) : Results.Problem(error, statusCode: 400);
});

api.MapPut("/wishes/{id:int}", async (HttpContext http, int id, WishInput input, WichtelService s) =>
{
    var (wish, error, notFound) = await s.UpdateWishAsync(Me(http), id, input);
    if (notFound) return Results.NotFound();
    return error is null ? Results.Ok(wish) : Results.Problem(error, statusCode: 400);
});

api.MapDelete("/wishes/{id:int}", async (HttpContext http, int id, WichtelService s) =>
{
    var (error, notFound) = await s.DeleteWishAsync(Me(http), id);
    if (notFound) return Results.NotFound();
    return error is null ? Results.NoContent() : Results.Problem(error, statusCode: 400);
});

api.MapPost("/reveal", async (HttpContext http, WichtelService s) =>
{
    var result = await s.RevealAsync(Me(http));
    return result is null
        ? Results.Problem("Die Ziehung startet erst am 20. November.", statusCode: StatusCodes.Status403Forbidden)
        : Results.Ok(result);
});

api.MapGet("/wichtel", async (HttpContext http, WichtelService s) =>
{
    var result = await s.GetWichtelAsync(Me(http));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

// Admin: personal links and resets, guarded by Wichteln:AdminKey.
var admin = app.MapGroup("/admin").AddEndpointFilter(async (ctx, next) =>
{
    var key = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<WichtelOptions>>().Value.AdminKey;
    var given = ctx.HttpContext.Request.Headers["X-Admin-Key"].ToString();
    if (string.IsNullOrEmpty(key) || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(given), System.Text.Encoding.UTF8.GetBytes(key)))
        return Results.Unauthorized();
    return await next(ctx);
});
admin.MapGet("/links", async (WichtelService s) => Results.Ok(await s.LinksAsync()));
admin.MapPost("/reset-draw", async (WichtelService s) => { await s.ResetDrawAsync(); return Results.NoContent(); });
admin.MapPost("/new-link/{name}", async (string name, WichtelService s) =>
    await s.NewLinkAsync(name) ? Results.Ok(await s.LinksAsync()) : Results.NotFound());

app.Run();

public partial class Program;
