namespace KnowledgeForge.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = [];
}
