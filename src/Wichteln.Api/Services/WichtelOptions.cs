namespace Wichteln.Api.Services;

public sealed class WichtelOptions
{
    /// <summary>When the "Zeige meinen Wichtel" button unlocks and wishlists freeze.</summary>
    public DateTimeOffset UnlockAt { get; set; } = new(2026, 11, 20, 0, 0, 0, TimeSpan.FromHours(1));

    /// <summary>Public address of the Blazor site, used to build personal links.</summary>
    public string PublicAppUrl { get; set; } = "http://localhost:5200";

    /// <summary>Key for the admin endpoints. Empty disables them.</summary>
    public string AdminKey { get; set; } = "";

    /// <summary>Origins allowed to call the API (the Blazor site).</summary>
    public string[] AllowedOrigins { get; set; } = [];

    // Defaults live in appsettings.json: the config binder appends to pre-filled arrays instead of replacing them.
    public string[] People { get; set; } = [];

    /// <summary>Pairs that may not draw each other, in either direction.</summary>
    public string[][] ForbiddenPairs { get; set; } = [];
}
