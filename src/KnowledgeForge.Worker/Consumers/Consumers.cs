using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Events;
using KnowledgeForge.Infrastructure.Services;
using MassTransit;

namespace KnowledgeForge.Worker.Consumers;

public class BookUploadedConsumer(BookProcessingService processingService, ILogger<BookUploadedConsumer> logger) : IConsumer<BookUploadedEvent>
{
    public async Task Consume(ConsumeContext<BookUploadedEvent> context)
    {
        logger.LogInformation("Processing book {BookId}", context.Message.BookId);
        await processingService.ProcessBookAsync(context.Message.BookId, context.Message.FilePath, context.CancellationToken);
        logger.LogInformation("Book {BookId} processed successfully", context.Message.BookId);
    }
}

public class GraphExtractionConsumer(IKnowledgeGraphExtractionService graphService, ILogger<GraphExtractionConsumer> logger) : IConsumer<GraphExtractionRequestedEvent>
{
    public async Task Consume(ConsumeContext<GraphExtractionRequestedEvent> context)
    {
        logger.LogInformation("Extracting knowledge graph for book {BookId}", context.Message.BookId);
        await graphService.ExtractGraphAsync(context.Message.BookId, context.CancellationToken);
        logger.LogInformation("Knowledge graph extracted for book {BookId}", context.Message.BookId);
    }
}
