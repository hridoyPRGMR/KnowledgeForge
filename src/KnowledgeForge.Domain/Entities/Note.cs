namespace KnowledgeForge.Domain.Entities;

public class Note
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid? ChapterId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
}
