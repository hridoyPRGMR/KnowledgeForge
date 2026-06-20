using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class SummaryEndpoints
{
    public static WebApplication MapSummaryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books/{bookId:guid}/chapters/{chapterId:guid}/summary", async (Guid bookId, Guid chapterId, ISummaryService summaries, CancellationToken ct) =>
        {
            var summary = await summaries.GetSummaryAsync(bookId, chapterId, ct);
            return summary is null ? Results.NotFound() : Results.Ok(summary);
        });

        app.MapPost("/api/books/{bookId:guid}/chapters/{chapterId:guid}/summarize", async (Guid bookId, Guid chapterId, ISummaryService summaries, CancellationToken ct) =>
            Results.Ok(await summaries.GenerateSummaryAsync(bookId, chapterId, ct)));

        return app;
    }
}
