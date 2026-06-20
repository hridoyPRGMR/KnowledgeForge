using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class RetrievalService(AppDbContext db) : IRetrievalService
{
    public async Task<IReadOnlyList<BookChunk>> SearchAsync(Guid bookId, Vector queryEmbedding, int topK, CancellationToken ct = default)
    {
        return await db.BookChunks
            .Include(c => c.Chapter)
            .Where(c => c.BookId == bookId && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BookChunk>> SearchAcrossBooksAsync(IReadOnlyList<Guid> bookIds, Vector queryEmbedding, int topK, CancellationToken ct = default)
    {
        return await db.BookChunks
            .Include(c => c.Chapter)
            .Include(c => c.Book)
            .Where(c => bookIds.Contains(c.BookId) && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .ToListAsync(ct);
    }
}
