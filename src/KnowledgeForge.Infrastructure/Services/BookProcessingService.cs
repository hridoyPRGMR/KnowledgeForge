using System.Text.Json;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Domain.Enums;
using KnowledgeForge.Domain.Events;
using KnowledgeForge.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeForge.Infrastructure.Services;

public class KnowledgeGraphExtractionService(AppDbContext db, IOllamaService ollama) : IKnowledgeGraphExtractionService
{
    public async Task ExtractGraphAsync(Guid bookId, CancellationToken ct = default)
    {
        var chapters = await db.BookChapters
            .Include(c => c.Chunks)
            .Where(c => c.BookId == bookId)
            .OrderBy(c => c.ChapterNumber)
            .ToListAsync(ct);

        var nodeMap = new Dictionary<string, KnowledgeNode>();

        foreach (var chapter in chapters.Take(3))
        {
            var content = string.Join("\n", chapter.Chunks.OrderBy(c => c.ChunkIndex).Take(5).Select(c => c.Content));
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var prompt = """
                Extract knowledge graph entities and relationships from this text.
                Return JSON: { "nodes": [{ "label": "...", "type": "Concept|Person|Theme|Event", "description": "..." }], "edges": [{ "source": "...", "target": "...", "relation": "relates_to|causes|example_of|contradicts" }] }

                Text:
                """ + content[..Math.Min(content.Length, 3000)];

            try
            {
                var json = await ollama.GenerateJsonAsync(prompt, ct);
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("nodes", out var nodesEl))
                {
                    foreach (var nodeEl in nodesEl.EnumerateArray())
                    {
                        var label = nodeEl.GetProperty("label").GetString() ?? string.Empty;
                        var normalized = label.Trim().ToLowerInvariant();
                        if (string.IsNullOrWhiteSpace(normalized) || nodeMap.ContainsKey(normalized))
                        {
                            continue;
                        }

                        Enum.TryParse<KnowledgeNodeType>(nodeEl.GetProperty("type").GetString(), true, out var nodeType);
                        var node = new KnowledgeNode
                        {
                            Id = Guid.NewGuid(),
                            BookId = bookId,
                            Label = label,
                            NormalizedLabel = normalized,
                            Type = nodeType,
                            Description = nodeEl.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty
                        };
                        nodeMap[normalized] = node;
                        db.KnowledgeNodes.Add(node);
                    }
                }

                await db.SaveChangesAsync(ct);

                if (root.TryGetProperty("edges", out var edgesEl))
                {
                    foreach (var edgeEl in edgesEl.EnumerateArray())
                    {
                        var source = edgeEl.GetProperty("source").GetString()?.Trim().ToLowerInvariant() ?? string.Empty;
                        var target = edgeEl.GetProperty("target").GetString()?.Trim().ToLowerInvariant() ?? string.Empty;
                        if (!nodeMap.TryGetValue(source, out var sourceNode) || !nodeMap.TryGetValue(target, out var targetNode))
                        {
                            continue;
                        }

                        db.KnowledgeEdges.Add(new KnowledgeEdge
                        {
                            Id = Guid.NewGuid(),
                            SourceNodeId = sourceNode.Id,
                            TargetNodeId = targetNode.Id,
                            RelationType = edgeEl.TryGetProperty("relation", out var rel) ? rel.GetString() ?? "relates_to" : "relates_to",
                            Weight = 1.0
                        });
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch
            {
                // Continue with other chapters if one fails
            }
        }
    }

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

public class BookProcessingService(
    AppDbContext db,
    IPdfProcessingService pdfService,
    IOllamaService ollama,
    IOptions<RagOptions> ragOptions,
    IBusPublisher busPublisher)
{
    private readonly RagOptions _rag = ragOptions.Value;

    public async Task ProcessBookAsync(Guid bookId, string filePath, CancellationToken ct = default)
    {
        var book = await db.Books.FindAsync([bookId], ct)
            ?? throw new KeyNotFoundException($"Book {bookId} not found.");

        book.Status = BookStatus.Processing;
        book.ErrorMessage = null;
        await db.SaveChangesAsync(ct);

        try
        {
            var extraction = await pdfService.ExtractAsync(filePath, _rag.ChunkSize, _rag.ChunkOverlap, ct);
            book.PageCount = extraction.PageCount;

            foreach (var chapterResult in extraction.Chapters)
            {
                var chapter = new BookChapter
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    ChapterNumber = chapterResult.ChapterNumber,
                    Title = chapterResult.Title,
                    StartPage = chapterResult.StartPage,
                    EndPage = chapterResult.EndPage
                };
                db.BookChapters.Add(chapter);

                var batch = new List<BookChunk>();
                foreach (var chunkResult in chapterResult.Chunks)
                {
                    batch.Add(new BookChunk
                    {
                        Id = Guid.NewGuid(),
                        BookId = bookId,
                        ChapterId = chapter.Id,
                        ChunkIndex = chunkResult.ChunkIndex,
                        Content = chunkResult.Content,
                        TokenCount = chunkResult.TokenCount
                    });
                }

                for (var i = 0; i < batch.Count; i += 10)
                {
                    var slice = batch.Skip(i).Take(10).ToList();
                    foreach (var chunk in slice)
                    {
                        chunk.Embedding = await ollama.GenerateEmbeddingAsync(chunk.Content, ct);
                    }
                    db.BookChunks.AddRange(slice);
                    await db.SaveChangesAsync(ct);
                }
            }

            book.Status = BookStatus.Ready;
            await db.SaveChangesAsync(ct);
            await busPublisher.PublishGraphExtractionAsync(bookId, ct);
        }
        catch (Exception ex)
        {
            book.Status = BookStatus.Failed;
            book.ErrorMessage = ex.Message;
            await db.SaveChangesAsync(ct);
            throw;
        }
    }
}

public interface IBusPublisher
{
    Task PublishGraphExtractionAsync(Guid bookId, CancellationToken ct = default);
}

public class MassTransitBusPublisher(IBus bus) : IBusPublisher
{
    public Task PublishGraphExtractionAsync(Guid bookId, CancellationToken ct = default) =>
        bus.Publish(new GraphExtractionRequestedEvent(bookId), ct);
}
