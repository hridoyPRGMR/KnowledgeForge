using KnowledgeForge.Domain.Enums;

namespace KnowledgeForge.Domain.Entities;

public class Quiz
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid ChapterId { get; set; }
    public QuizType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
    public BookChapter Chapter { get; set; } = null!;
    public ICollection<QuizQuestion> Questions { get; set; } = [];
}
