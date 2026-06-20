using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Domain.Enums;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeForge.Infrastructure.Services;

public class ChatService(
    AppDbContext db,
    IRetrievalService retrieval,
    IOllamaService ollama,
    ICacheService cache,
    IOptions<RagOptions> ragOptions) : IChatService
{
    private readonly RagOptions _rag = ragOptions.Value;

    public async Task<ChatResponseDto> AskAsync(Guid bookId, ChatRequestDto request, CancellationToken ct = default)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct)
            ?? throw new KeyNotFoundException($"Book {bookId} not found.");

        if (book.Status != BookStatus.Ready)
        {
            throw new InvalidOperationException("Book is not ready for chat yet.");
        }

        var cacheKey = $"chat:{bookId}:{HashQuestion(request.Message)}";
        var cached = await cache.GetAsync(cacheKey, ct);
        if (cached is not null)
        {
            var cachedResponse = JsonSerializer.Deserialize<ChatResponseDto>(cached)!;
            return cachedResponse;
        }

        var embedding = await ollama.GenerateEmbeddingAsync(request.Message, ct);
        var chunks = await retrieval.SearchAsync(bookId, embedding, _rag.TopK, ct);

        var context = string.Join("\n\n---\n\n", chunks.Select((c, i) => $"[{i + 1}] {c.Content}"));
        var prompt = $"""
            Use ONLY the provided context. If the answer is not in the context, say so clearly.

            Context:
            {context}

            Question: {request.Message}
            """;

        var answer = await ollama.GenerateChatAsync(prompt, ct);

        var conversation = request.ConversationId.HasValue
            ? await db.Conversations.FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.BookId == bookId, ct)
            : null;

        if (conversation is null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                Title = request.Message.Length > 50 ? request.Message[..50] + "..." : request.Message
            };
            db.Conversations.Add(conversation);
        }

        var sources = chunks.Select(c => new SourceChunkDto(c.Id, c.Content, c.ChunkIndex, c.Chapter.Title)).ToList();
        var sourcesJson = JsonSerializer.Serialize(sources);

        db.Messages.Add(new Message { Id = Guid.NewGuid(), ConversationId = conversation.Id, Role = MessageRole.User, Content = request.Message });
        db.Messages.Add(new Message { Id = Guid.NewGuid(), ConversationId = conversation.Id, Role = MessageRole.Assistant, Content = answer, SourceChunksJson = sourcesJson });
        await db.SaveChangesAsync(ct);

        var response = new ChatResponseDto(conversation.Id, answer, sources);
        await cache.SetAsync(cacheKey, JsonSerializer.Serialize(response), TimeSpan.FromHours(1), ct);
        return response;
    }

    public async Task<CrossBookChatResponseDto> AskCrossBookAsync(CrossBookChatRequestDto request, CancellationToken ct = default)
    {
        if (request.BookIds.Count == 0)
        {
            throw new ArgumentException("At least one book must be selected.");
        }

        var books = await db.Books
            .Where(b => request.BookIds.Contains(b.Id) && b.Status == BookStatus.Ready)
            .ToListAsync(ct);

        if (books.Count == 0)
        {
            throw new InvalidOperationException("No ready books found for cross-book reasoning.");
        }

        var embedding = await ollama.GenerateEmbeddingAsync(request.Message, ct);
        var chunks = await retrieval.SearchAcrossBooksAsync(request.BookIds, embedding, _rag.TopK * 2, ct);

        var graphContext = await BuildGraphContextAsync(request.BookIds, ct);
        var chunkContext = string.Join("\n\n---\n\n", chunks.Select(c => $"[Book: {c.Book.Title} | Chapter: {c.Chapter.Title}]\n{c.Content}"));

        var prompt = $"""
            You are analyzing multiple books. Use ONLY the provided context from all books.
            Highlight connections between concepts across different books when relevant.

            Book Chunks:
            {chunkContext}

            Knowledge Graph Connections:
            {graphContext}

            Question: {request.Message}
            """;

        var answer = await ollama.GenerateChatAsync(prompt, ct);
        var sources = chunks.Select(c => new CrossBookSourceDto(c.BookId, c.Book.Title, c.Id, c.Content, c.Chapter.Title)).ToList();
        return new CrossBookChatResponseDto(answer, sources);
    }

    private async Task<string> BuildGraphContextAsync(IReadOnlyList<Guid> bookIds, CancellationToken ct)
    {
        var edges = await db.KnowledgeEdges
            .Include(e => e.SourceNode)
            .Include(e => e.TargetNode)
            .Where(e => bookIds.Contains(e.SourceNode.BookId))
            .Take(20)
            .ToListAsync(ct);

        if (edges.Count == 0)
        {
            return "No graph connections available.";
        }

        return string.Join("\n", edges.Select(e => $"- {e.SourceNode.Label} --[{e.RelationType}]--> {e.TargetNode.Label}"));
    }

    private static string HashQuestion(string question)
    {
        var normalized = question.Trim().ToLowerInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }
}
