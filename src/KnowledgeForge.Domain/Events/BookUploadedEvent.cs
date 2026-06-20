namespace KnowledgeForge.Domain.Events;

public record BookUploadedEvent(Guid BookId, string FilePath);
