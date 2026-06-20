using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class ProfileEndpoints
{
    public static WebApplication MapProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books/{bookId:guid}/progress", async (Guid bookId, ILearningProfileService profile, CancellationToken ct) =>
        {
            var progress = await profile.GetProgressAsync(bookId, ct);
            return progress is null ? Results.NotFound() : Results.Ok(progress);
        });

        app.MapPatch("/api/books/{bookId:guid}/progress", async (Guid bookId, UpdateProgressDto dto, ILearningProfileService profile, CancellationToken ct) =>
            Results.Ok(await profile.UpdateProgressAsync(bookId, dto, ct)));

        app.MapGet("/api/profile", async (ILearningProfileService profile, CancellationToken ct) =>
            Results.Ok(await profile.GetProfileAsync(ct)));

        return app;
    }
}
