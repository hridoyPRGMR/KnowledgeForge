using System.Text.Json;
using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class SummaryService(AppDbContext db, IChatCompletionService chat, ICacheService cache) : ISummaryService
{
    public async Task<SummaryDto?> GetSummaryAsync(Guid bookId, Guid chapterId, CancellationToken ct = default)
    {
        var summary = await db.Summaries
            .FirstOrDefaultAsync(s => s.BookId == bookId && s.ChapterId == chapterId, ct);

        return summary is null ? null : Map(summary);
    }

    public async Task<SummaryDto> GenerateSummaryAsync(Guid bookId, Guid chapterId, CancellationToken ct = default)
    {
        var existing = await GetSummaryAsync(bookId, chapterId, ct);
        if (existing is not null)
        {
            return existing;
        }

        var chapter = await db.BookChapters
            .Include(c => c.Chunks)
            .FirstOrDefaultAsync(c => c.BookId == bookId && c.Id == chapterId, ct)
            ?? throw new KeyNotFoundException("Chapter not found.");

        var content = string.Join("\n\n", chapter.Chunks.OrderBy(c => c.ChunkIndex).Select(c => c.Content));
        var prompt = $"""
            Summarize the following chapter text. Return JSON with keys: summaryText, keyIdeas, actionItems.
            keyIdeas and actionItems should be newline-separated bullet points.

            Chapter: {chapter.Title}
            Text:
            {content}
            """;

        var json = await chat.GenerateJsonAsync(prompt, ct);
        json = CleanJson(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var summary = new Summary
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ChapterId = chapterId,
            SummaryText = root.GetProperty("summaryText").GetString() ?? string.Empty,
            KeyIdeas = root.GetProperty("keyIdeas").GetString() ?? string.Empty,
            ActionItems = root.GetProperty("actionItems").GetString() ?? string.Empty
        };

        db.Summaries.Add(summary);
        await db.SaveChangesAsync(ct);

        var dto = Map(summary);
        await cache.SetAsync($"summary:{bookId}:{chapterId}", JsonSerializer.Serialize(dto), TimeSpan.FromDays(7), ct);
        return dto;
    }

    private static SummaryDto Map(Summary s) => new(s.Id, s.SummaryText, s.KeyIdeas, s.ActionItems);

    private static string CleanJson(string json)
    {
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                json = json[start..(end + 1)];
            }
        }
        return json;
    }
}
