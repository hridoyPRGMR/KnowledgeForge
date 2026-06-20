using KnowledgeForge.Domain.Enums;

namespace KnowledgeForge.Application.DTOs;

public record BookDto(Guid Id, string Title, BookStatus Status, int PageCount, DateTime CreatedAt, int ChapterCount);
public record BookDetailDto(Guid Id, string Title, BookStatus Status, int PageCount, string? ErrorMessage, DateTime CreatedAt, IReadOnlyList<ChapterDto> Chapters);
public record ChapterDto(Guid Id, int ChapterNumber, string Title, int StartPage, int EndPage);
public record BookStatusDto(Guid Id, BookStatus Status, string? ErrorMessage, int ChunkCount);

public record ChatRequestDto(Guid? ConversationId, string Message);
public record ChatResponseDto(Guid ConversationId, string Answer, IReadOnlyList<SourceChunkDto> Sources);
public record SourceChunkDto(Guid ChunkId, string Content, int ChunkIndex, string ChapterTitle);

public record SummaryDto(Guid Id, string SummaryText, string KeyIdeas, string ActionItems);
public record QuizDto(Guid Id, QuizType Type, IReadOnlyList<QuizQuestionDto> Questions);
public record QuizQuestionDto(Guid Id, string Question, IReadOnlyList<string> Options, string CorrectAnswer, string Explanation);

public record NoteDto(Guid Id, Guid BookId, Guid? ChapterId, string Content, DateTime CreatedAt);
public record CreateNoteDto(Guid? ChapterId, string Content);
public record HighlightDto(Guid Id, Guid BookId, Guid? ChapterId, string Text, int StartOffset, int EndOffset, string Color, DateTime CreatedAt);
public record CreateHighlightDto(Guid? ChapterId, string Text, int StartOffset, int EndOffset, string Color);

public record ReadingProgressDto(Guid BookId, string BookTitle, int CurrentChapter, int CurrentPage, double PercentComplete, DateTime LastReadAt);
public record UpdateProgressDto(int CurrentChapter, int CurrentPage, double PercentComplete);
public record ProfileDto(int TotalBooks, int ReadyBooks, int ChaptersRead, double AverageProgress, IReadOnlyList<ReadingProgressDto> RecentBooks);

public record KnowledgeGraphDto(IReadOnlyList<KnowledgeNodeDto> Nodes, IReadOnlyList<KnowledgeEdgeDto> Edges);
public record KnowledgeNodeDto(Guid Id, string Label, string Type, string Description);
public record KnowledgeEdgeDto(Guid Id, Guid SourceId, Guid TargetId, string RelationType, double Weight);

public record CrossBookChatRequestDto(IReadOnlyList<Guid> BookIds, string Message);
public record CrossBookChatResponseDto(string Answer, IReadOnlyList<CrossBookSourceDto> Sources);
public record CrossBookSourceDto(Guid BookId, string BookTitle, Guid ChunkId, string Content, string ChapterTitle);

public record ChapterContentDto(Guid Id, string Title, int ChapterNumber, string Content);
