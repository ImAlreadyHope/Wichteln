using Microsoft.EntityFrameworkCore;

namespace Wichteln.Api.Data;

public sealed class Participant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    /// <summary>Secret part of the personal link.</summary>
    public required string Token { get; set; }
    public DateTimeOffset? RevealedAt { get; set; }
    public List<Wish> Wishes { get; set; } = [];
}

public sealed class Wish
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public required string Title { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>One drawn pair: Giver buys a present for Receiver.</summary>
public sealed class Assignment
{
    public int GiverId { get; set; }
    public int ReceiverId { get; set; }
}

public sealed class WichtelDb(DbContextOptions<WichtelDb> options) : DbContext(options)
{
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Wish> Wishes => Set<Wish>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Participant>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(60);
            e.Property(p => p.Token).HasMaxLength(64);
            e.HasIndex(p => p.Name).IsUnique();
            e.HasIndex(p => p.Token).IsUnique();
        });
        b.Entity<Wish>(e =>
        {
            e.Property(w => w.Title).HasMaxLength(Shared.WishRules.MaxTitleLength);
            e.Property(w => w.Url).HasMaxLength(Shared.WishRules.MaxUrlLength);
            e.HasOne(w => w.Participant).WithMany(p => p.Wishes).HasForeignKey(w => w.ParticipantId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Assignment>(e =>
        {
            e.HasKey(a => a.GiverId);
            // Each person can be drawn only once, enforced by the database too.
            e.HasIndex(a => a.ReceiverId).IsUnique();
            e.HasOne<Participant>().WithMany().HasForeignKey(a => a.GiverId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Participant>().WithMany().HasForeignKey(a => a.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
