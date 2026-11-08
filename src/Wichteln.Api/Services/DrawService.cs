namespace Wichteln.Api.Services;

/// <summary>Pure draw logic: who gives a present to whom.</summary>
public static class DrawService
{
    /// <summary>
    /// Returns giver → receiver for every id. Nobody draws themselves, every receiver
    /// appears exactly once, and no pair in <paramref name="forbidden"/> is drawn in either direction.
    /// The result is chosen uniformly from all valid assignments.
    /// </summary>
    public static Dictionary<int, int> Generate(IReadOnlyList<int> ids, IEnumerable<(int A, int B)> forbidden, Random rng)
    {
        if (ids.Count < 2) throw new InvalidOperationException("Mindestens zwei Personen werden gebraucht.");
        if (ids.Distinct().Count() != ids.Count) throw new ArgumentException("Doppelte Personen.", nameof(ids));

        var blocked = new HashSet<(int, int)>();
        foreach (var (a, b) in forbidden) { blocked.Add((a, b)); blocked.Add((b, a)); }

        var all = new List<Dictionary<int, int>>();
        Enumerate(ids, blocked, 0, new Dictionary<int, int>(), new HashSet<int>(), all);
        if (all.Count == 0) throw new InvalidOperationException("Mit diesen Regeln ist keine Ziehung möglich.");
        return all[rng.Next(all.Count)];
    }

    /// <summary>Checks an assignment against every rule.</summary>
    public static bool IsValid(IReadOnlyList<int> ids, IReadOnlyDictionary<int, int> map, IEnumerable<(int A, int B)> forbidden)
    {
        var blocked = new HashSet<(int, int)>();
        foreach (var (a, b) in forbidden) { blocked.Add((a, b)); blocked.Add((b, a)); }
        if (map.Count != ids.Count || !ids.All(map.ContainsKey)) return false;
        if (map.Values.Distinct().Count() != ids.Count || !map.Values.All(ids.Contains)) return false;
        return map.All(kv => kv.Key != kv.Value && !blocked.Contains((kv.Key, kv.Value)));
    }

    // Small groups only (a family): exhaustive enumeration keeps the choice uniform and provably valid.
    private static void Enumerate(IReadOnlyList<int> ids, HashSet<(int, int)> blocked, int i,
        Dictionary<int, int> current, HashSet<int> used, List<Dictionary<int, int>> results)
    {
        if (ids.Count > 9) throw new InvalidOperationException("Für so viele Personen ist diese Ziehung nicht gedacht.");
        if (i == ids.Count) { results.Add(new Dictionary<int, int>(current)); return; }
        var giver = ids[i];
        foreach (var receiver in ids)
        {
            if (receiver == giver || used.Contains(receiver) || blocked.Contains((giver, receiver))) continue;
            current[giver] = receiver; used.Add(receiver);
            Enumerate(ids, blocked, i + 1, current, used, results);
            current.Remove(giver); used.Remove(receiver);
        }
    }
}
