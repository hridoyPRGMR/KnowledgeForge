namespace KnowledgeForge.Domain.Entities;

public class BookChapter
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public int StartPage { get; set; }
    public int EndPage { get; set; }

    public Book Book { get; set; } = null!;
    public ICollection<BookChunk> Chunks { get; set; } = [];
    public ICollection<Summary> Summaries { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];
}
