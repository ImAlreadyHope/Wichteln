using Wichteln.Api.Services;

namespace Wichteln.Tests;

public class DrawServiceTests
{
    // Justin=1, Luana=2, Lydia=3, Angelika=4, Denis=5
    private static readonly int[] Ids = [1, 2, 3, 4, 5];
    private static readonly (int, int)[] Forbidden = [(1, 2), (4, 5)];

    [Fact]
    public void Every_draw_follows_all_rules()
    {
        var rng = new Random(42);
        for (var i = 0; i < 5000; i++)
        {
            var map = DrawService.Generate(Ids, Forbidden, rng);
            Assert.True(DrawService.IsValid(Ids, map, Forbidden));
            Assert.NotEqual(2, map[1]); Assert.NotEqual(1, map[2]);
            Assert.NotEqual(5, map[4]); Assert.NotEqual(4, map[5]);
            Assert.Equal(5, map.Values.Distinct().Count());
            Assert.All(map, kv => Assert.NotEqual(kv.Key, kv.Value));
        }
    }

    [Fact]
    public void Draws_vary_and_every_allowed_pair_can_happen()
    {
        var rng = new Random(7);
        var seen = new HashSet<(int, int)>();
        for (var i = 0; i < 5000; i++)
            foreach (var kv in DrawService.Generate(Ids, Forbidden, rng)) seen.Add((kv.Key, kv.Value));
        // 5*4 ordered pairs minus 4 forbidden directions = 16 possible pairs.
        Assert.Equal(16, seen.Count);
    }

    [Fact]
    public void Impossible_rules_throw()
    {
        Assert.Throws<InvalidOperationException>(() => DrawService.Generate([1, 2], [(1, 2)], new Random()));
    }

    [Fact]
    public void IsValid_rejects_duplicates_and_forbidden_pairs()
    {
        Assert.False(DrawService.IsValid(Ids, new Dictionary<int, int> { [1] = 3, [2] = 3, [3] = 1, [4] = 1, [5] = 2 }, Forbidden));
        Assert.False(DrawService.IsValid(Ids, new Dictionary<int, int> { [1] = 2, [2] = 3, [3] = 4, [4] = 5, [5] = 1 }, Forbidden));
        Assert.True(DrawService.IsValid(Ids, new Dictionary<int, int> { [1] = 4, [4] = 2, [2] = 5, [5] = 3, [3] = 1 }, Forbidden));
    }
}
