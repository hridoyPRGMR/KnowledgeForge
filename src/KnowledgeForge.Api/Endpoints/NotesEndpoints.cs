using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class NotesEndpoints
{
    public static WebApplication MapNotesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books/{bookId:guid}/notes", async (Guid bookId, INotesService notes, CancellationToken ct) =>
            Results.Ok(await notes.GetNotesAsync(bookId, ct)));

        app.MapPost("/api/books/{bookId:guid}/notes", async (Guid bookId, CreateNoteDto dto, INotesService notes, CancellationToken ct) =>
            Results.Ok(await notes.CreateNoteAsync(bookId, dto, ct)));

        app.MapDelete("/api/notes/{id:guid}", async (Guid id, INotesService notes, CancellationToken ct) =>
        {
            await notes.DeleteNoteAsync(id, ct);
            return Results.NoContent();
        });

        app.MapGet("/api/books/{bookId:guid}/highlights", async (Guid bookId, INotesService notes, CancellationToken ct) =>
            Results.Ok(await notes.GetHighlightsAsync(bookId, ct)));

        app.MapPost("/api/books/{bookId:guid}/highlights", async (Guid bookId, CreateHighlightDto dto, INotesService notes, CancellationToken ct) =>
            Results.Ok(await notes.CreateHighlightAsync(bookId, dto, ct)));

        app.MapDelete("/api/highlights/{id:guid}", async (Guid id, INotesService notes, CancellationToken ct) =>
        {
            await notes.DeleteHighlightAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }
}
