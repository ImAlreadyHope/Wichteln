namespace Wichteln.Client.Components;

/// <summary>In-house 24px icons from the design system (2px round stroke, currentColor).</summary>
public static class Icons
{
    public static readonly IReadOnlyDictionary<string, string> Paths = new Dictionary<string, string>
    {
        ["gift"] = """<rect x="3" y="8" width="18" height="4" rx="1.5"/><path d="M5 12v7a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-7"/><path d="M12 8v13"/><path d="M12 8c-1.5-3-5-4-5.5-2S9 8 12 8Zm0 0c1.5-3 5-4 5.5-2S15 8 12 8Z"/>""",
        ["lock"] = """<rect x="4" y="11" width="16" height="10" rx="2.5"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/><path d="M12 15v2"/>""",
        ["plus"] = """<path d="M12 5v14M5 12h14"/>""",
        ["link"] = """<path d="M10 14a4 4 0 0 0 5.66 0l3-3a4 4 0 0 0-5.66-5.66l-1 1"/><path d="M14 10a4 4 0 0 0-5.66 0l-3 3a4 4 0 0 0 5.66 5.66l1-1"/>""",
        ["trash"] = """<path d="M4 7h16"/><path d="M9 7V5a1.5 1.5 0 0 1 1.5-1.5h3A1.5 1.5 0 0 1 15 5v2"/><path d="M6.5 7l.8 12a2 2 0 0 0 2 1.9h5.4a2 2 0 0 0 2-1.9l.8-12"/>""",
        ["chevron-right"] = """<path d="M9 6l6 6-6 6"/>""",
        ["external"] = """<path d="M14 4h6v6"/><path d="M20 4l-9 9"/><path d="M18 14v4a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4"/>""",
        ["sparkles"] = """<path d="M12 3l1.8 4.7L18.5 9.5l-4.7 1.8L12 16l-1.8-4.7L5.5 9.5l4.7-1.8Z"/><path d="M19 15l.7 1.8 1.8.7-1.8.7L19 20l-.7-1.8-1.8-.7 1.8-.7Z"/>""",
        ["calendar"] = """<rect x="3.5" y="5" width="17" height="15.5" rx="2.5"/><path d="M8 3v4M16 3v4M3.5 10h17"/>""",
        ["check"] = """<path d="M5 12.5l4.5 4.5L19 7.5"/>""",
        ["pencil"] = """<path d="M4 20l1-4L16 5a2.1 2.1 0 0 1 3 3L8 19Z"/><path d="M14 7l3 3"/>""",
        ["alert"] = """<circle cx="12" cy="12" r="9"/><path d="M12 7.5v5.5M12 16.5v.01"/>"""
    };
}
