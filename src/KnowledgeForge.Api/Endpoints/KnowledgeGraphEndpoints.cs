using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class KnowledgeGraphEndpoints
{
    public static WebApplication MapKnowledgeGraphEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books/{bookId:guid}/graph", async (Guid bookId, IKnowledgeGraphService graph, CancellationToken ct) =>
        {
            var result = await graph.GetGraphAsync(bookId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        return app;
    }
}
