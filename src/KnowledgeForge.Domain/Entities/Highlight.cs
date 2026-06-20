namespace KnowledgeForge.Domain.Entities;

public class Highlight
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid? ChapterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public string Color { get; set; } = "#FFEB3B";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
}
