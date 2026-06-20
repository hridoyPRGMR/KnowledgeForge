namespace KnowledgeForge.Domain.Entities;

public class Summary
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid ChapterId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public string KeyIdeas { get; set; } = string.Empty;
    public string ActionItems { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
    public BookChapter Chapter { get; set; } = null!;
}
