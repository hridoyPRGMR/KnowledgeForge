using KnowledgeForge.Domain.Enums;

namespace KnowledgeForge.Domain.Entities;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public BookStatus Status { get; set; } = BookStatus.Uploaded;
    public int PageCount { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookChapter> Chapters { get; set; } = [];
    public ICollection<BookChunk> Chunks { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Highlight> Highlights { get; set; } = [];
    public ICollection<KnowledgeNode> KnowledgeNodes { get; set; } = [];
    public ReadingProgress? ReadingProgress { get; set; }
}
