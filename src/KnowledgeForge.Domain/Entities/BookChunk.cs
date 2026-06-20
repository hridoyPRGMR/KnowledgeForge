using Pgvector;

namespace KnowledgeForge.Domain.Entities;

public class BookChunk
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid ChapterId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public int TokenCount { get; set; }

    public Book Book { get; set; } = null!;
    public BookChapter Chapter { get; set; } = null!;
}
