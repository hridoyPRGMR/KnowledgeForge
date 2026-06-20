namespace KnowledgeForge.Domain.Entities;

public class ReadingProgress
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public int CurrentChapter { get; set; }
    public int CurrentPage { get; set; }
    public double PercentComplete { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
}
