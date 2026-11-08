namespace Wichteln.Shared;

/// <summary>A wish on someone's list.</summary>
public sealed record WishDto(int Id, string Title, string? Url);

/// <summary>Body for creating or editing a wish.</summary>
public sealed record WishInput(string Title, string? Url);

/// <summary>A participant as everyone may see them (no token).</summary>
public sealed record PersonDto(int Id, string Name);

/// <summary>Everything the home screen needs for the signed-in person.</summary>
public sealed record MeDto(
    int Id,
    string Name,
    DateTimeOffset UnlockAt,
    DateTimeOffset ServerNow,
    bool IsUnlocked,
    bool HasRevealed,
    IReadOnlyList<WishDto> Wishes,
    IReadOnlyList<PersonDto> Others);

/// <summary>The person you give a present to, and their wishlist.</summary>
public sealed record WichtelDto(PersonDto Person, IReadOnlyList<WishDto> Wishes);

public static class WishRules
{
    public const int MaxTitleLength = 200;
    public const int MaxUrlLength = 1000;
    public const int MaxWishes = 30;

    /// <summary>Returns an error message in German, or null when valid.</summary>
    public static string? Validate(WishInput input)
    {
        var title = input.Title?.Trim() ?? "";
        if (title.Length == 0) return "Bitte gib einen Wunsch ein.";
        if (title.Length > MaxTitleLength) return $"Der Wunsch darf höchstens {MaxTitleLength} Zeichen haben.";
        var url = input.Url?.Trim();
        if (string.IsNullOrEmpty(url)) return null;
        if (url.Length > MaxUrlLength) return "Der Link ist zu lang.";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return "Der Link muss mit https:// beginnen.";
        return null;
    }
}
