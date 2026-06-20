using System.Text.Json;
using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Domain.Enums;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class QuizService(AppDbContext db, IChatCompletionService chat) : IQuizService
{
    public async Task<QuizDto?> GetQuizAsync(Guid bookId, Guid chapterId, QuizType type, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.BookId == bookId && q.ChapterId == chapterId && q.Type == type, ct);

        return quiz is null ? null : Map(quiz);
    }

    public async Task<QuizDto> GenerateQuizAsync(Guid bookId, Guid chapterId, QuizType type, CancellationToken ct = default)
    {
        var existing = await GetQuizAsync(bookId, chapterId, type, ct);
        if (existing is not null)
        {
            return existing;
        }

        var chapter = await db.BookChapters
            .Include(c => c.Chunks)
            .FirstOrDefaultAsync(c => c.BookId == bookId && c.Id == chapterId, ct)
            ?? throw new KeyNotFoundException("Chapter not found.");

        var content = string.Join("\n\n", chapter.Chunks.OrderBy(c => c.ChunkIndex).Select(c => c.Content));
        var typeName = type.ToString().ToLowerInvariant();

        var prompt = $"""
            Generate a {typeName} quiz from this chapter. Return JSON array with 5 items.
            Each item: question, options array, correctAnswer, explanation fields.
            For flashcards, options can be empty array. For truefalse, options should be True and False.

            Chapter: {chapter.Title}
            Text:
            {content}
            """;

        var json = await chat.GenerateJsonAsync(prompt, ct);
        json = CleanJson(json);

        using var doc = JsonDocument.Parse(json);
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ChapterId = chapterId,
            Type = type
        };

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var options = item.TryGetProperty("options", out var opts)
                ? opts.EnumerateArray().Select(o => o.GetString() ?? string.Empty).ToList()
                : [];

            quiz.Questions.Add(new QuizQuestion
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                Question = item.GetProperty("question").GetString() ?? string.Empty,
                OptionsJson = JsonSerializer.Serialize(options),
                CorrectAnswer = item.GetProperty("correctAnswer").GetString() ?? string.Empty,
                Explanation = item.TryGetProperty("explanation", out var exp) ? exp.GetString() ?? string.Empty : string.Empty
            });
        }

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(ct);
        return Map(quiz);
    }

    private static QuizDto Map(Quiz quiz)
    {
        var questions = quiz.Questions.Select(q =>
        {
            var options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? [];
            return new QuizQuestionDto(q.Id, q.Question, options, q.CorrectAnswer, q.Explanation);
        }).ToList();

        return new QuizDto(quiz.Id, quiz.Type, questions);
    }

    private static string CleanJson(string json)
    {
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            var start = json.IndexOf('[');
            var end = json.LastIndexOf(']');
            if (start >= 0 && end > start)
            {
                json = json[start..(end + 1)];
            }
        }
        return json;
    }
}
