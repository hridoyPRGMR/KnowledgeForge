using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Enums;

namespace KnowledgeForge.Api.Endpoints;

public static class QuizEndpoints
{
    public static WebApplication MapQuizEndpoints(this WebApplication app)
    {
        app.MapGet("/api/books/{bookId:guid}/chapters/{chapterId:guid}/quiz/{type}", async (Guid bookId, Guid chapterId, string type, IQuizService quizzes, CancellationToken ct) =>
        {
            if (!Enum.TryParse<QuizType>(type, true, out var quizType))
            {
                return Results.BadRequest(new { error = "Invalid quiz type." });
            }

            var quiz = await quizzes.GetQuizAsync(bookId, chapterId, quizType, ct);
            return quiz is null ? Results.NotFound() : Results.Ok(quiz);
        });

        app.MapPost("/api/books/{bookId:guid}/chapters/{chapterId:guid}/quiz", async (Guid bookId, Guid chapterId, string? type, IQuizService quizzes, CancellationToken ct) =>
        {
            if (!Enum.TryParse<QuizType>(type ?? "mcq", true, out var quizType))
            {
                return Results.BadRequest(new { error = "Invalid quiz type." });
            }

            return Results.Ok(await quizzes.GenerateQuizAsync(bookId, chapterId, quizType, ct));
        });

        return app;
    }
}
