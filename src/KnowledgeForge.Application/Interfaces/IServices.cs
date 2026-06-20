using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Domain.Entities;
using Pgvector;

namespace KnowledgeForge.Application.Interfaces;

public interface IBookService
{
    Task<BookDto> UploadBookAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<IReadOnlyList<BookDto>> GetBooksAsync(CancellationToken ct = default);
    Task<BookDetailDto?> GetBookAsync(Guid id, CancellationToken ct = default);
    Task<BookStatusDto?> GetBookStatusAsync(Guid id, CancellationToken ct = default);
    Task<ChapterContentDto?> GetChapterContentAsync(Guid bookId, Guid chapterId, CancellationToken ct = default);
}

public interface IChatService
{
    Task<ChatResponseDto> AskAsync(Guid bookId, ChatRequestDto request, CancellationToken ct = default);
    Task<CrossBookChatResponseDto> AskCrossBookAsync(CrossBookChatRequestDto request, CancellationToken ct = default);
}

public interface ISummaryService
{
    Task<SummaryDto?> GetSummaryAsync(Guid bookId, Guid chapterId, CancellationToken ct = default);
    Task<SummaryDto> GenerateSummaryAsync(Guid bookId, Guid chapterId, CancellationToken ct = default);
}

public interface IQuizService
{
    Task<QuizDto?> GetQuizAsync(Guid bookId, Guid chapterId, Domain.Enums.QuizType type, CancellationToken ct = default);
    Task<QuizDto> GenerateQuizAsync(Guid bookId, Guid chapterId, Domain.Enums.QuizType type, CancellationToken ct = default);
}

public interface INotesService
{
    Task<NoteDto> CreateNoteAsync(Guid bookId, CreateNoteDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<NoteDto>> GetNotesAsync(Guid bookId, CancellationToken ct = default);
    Task DeleteNoteAsync(Guid id, CancellationToken ct = default);
    Task<HighlightDto> CreateHighlightAsync(Guid bookId, CreateHighlightDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<HighlightDto>> GetHighlightsAsync(Guid bookId, CancellationToken ct = default);
    Task DeleteHighlightAsync(Guid id, CancellationToken ct = default);
}

public interface ILearningProfileService
{
    Task<ReadingProgressDto?> GetProgressAsync(Guid bookId, CancellationToken ct = default);
    Task<ReadingProgressDto> UpdateProgressAsync(Guid bookId, UpdateProgressDto dto, CancellationToken ct = default);
    Task<ProfileDto> GetProfileAsync(CancellationToken ct = default);
}

public interface IKnowledgeGraphService
{
    Task<KnowledgeGraphDto?> GetGraphAsync(Guid bookId, CancellationToken ct = default);
}

public interface IRetrievalService
{
    Task<IReadOnlyList<BookChunk>> SearchAsync(Guid bookId, Vector queryEmbedding, int topK, CancellationToken ct = default);
    Task<IReadOnlyList<BookChunk>> SearchAcrossBooksAsync(IReadOnlyList<Guid> bookIds, Vector queryEmbedding, int topK, CancellationToken ct = default);
}

public interface IOllamaService
{
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task<string> GenerateChatAsync(string prompt, CancellationToken ct = default);
    Task<string> GenerateJsonAsync(string prompt, CancellationToken ct = default);
}

public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default);
}

public interface IPdfProcessingService
{
    Task<PdfExtractionResult> ExtractAsync(string filePath, int chunkSize, int chunkOverlap, CancellationToken ct = default);
}

public record PdfExtractionResult(int PageCount, IReadOnlyList<PdfChapterResult> Chapters);
public record PdfChapterResult(int ChapterNumber, string Title, int StartPage, int EndPage, string Content, IReadOnlyList<PdfChunkResult> Chunks);
public record PdfChunkResult(int ChunkIndex, string Content, int TokenCount);

public interface IKnowledgeGraphExtractionService
{
    Task ExtractGraphAsync(Guid bookId, CancellationToken ct = default);
}
