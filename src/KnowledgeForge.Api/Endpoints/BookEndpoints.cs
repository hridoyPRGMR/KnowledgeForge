using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class BookEndpoints
{
    public static WebApplication MapBookEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books", async (IBookService books, CancellationToken ct) =>
            Results.Ok(await books.GetBooksAsync(ct)));

        app.MapGet("/api/books/{id:guid}", async (Guid id, IBookService books, CancellationToken ct) =>
        {
            var book = await books.GetBookAsync(id, ct);
            return book is null ? Results.NotFound() : Results.Ok(book);
        });

        app.MapGet("/api/books/{id:guid}/status", async (Guid id, IBookService books, CancellationToken ct) =>
        {
            var status = await books.GetBookStatusAsync(id, ct);
            return status is null ? Results.NotFound() : Results.Ok(status);
        });

        app.MapPost("/api/books/upload", async (HttpRequest request, IBookService books, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Multipart form data required.");
            }

            var file = request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("PDF file is required.");
            }

            await using var stream = file.OpenReadStream();
            var book = await books.UploadBookAsync(stream, file.FileName, ct);
            return Results.Created($"/api/books/{book.Id}", book);
        }).DisableAntiforgery();

        app.MapGet("/api/books/{bookId:guid}/chapters/{chapterId:guid}/content", async (Guid bookId, Guid chapterId, IBookService books, CancellationToken ct) =>
        {
            var content = await books.GetChapterContentAsync(bookId, chapterId, ct);
            return content is null ? Results.NotFound() : Results.Ok(content);
        });

        return app;
    }
}
