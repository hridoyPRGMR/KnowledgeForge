using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class KnowledgeGraphService(AppDbContext db) : IKnowledgeGraphService
{
    public async Task<KnowledgeGraphDto?> GetGraphAsync(Guid bookId, CancellationToken ct = default)
    {
        var nodes = await db.KnowledgeNodes
            .Where(n => n.BookId == bookId)
            .ToListAsync(ct);

        if (nodes.Count == 0)
        {
            return null;
        }

        var nodeIds = nodes.Select(n => n.Id).ToHashSet();
        var edges = await db.KnowledgeEdges
            .Where(e => nodeIds.Contains(e.SourceNodeId) && nodeIds.Contains(e.TargetNodeId))
            .ToListAsync(ct);

        return new KnowledgeGraphDto(
            nodes.Select(n => new KnowledgeNodeDto(n.Id, n.Label, n.Type.ToString(), n.Description)).ToList(),
            edges.Select(e => new KnowledgeEdgeDto(e.Id, e.SourceNodeId, e.TargetNodeId, e.RelationType, e.Weight)).ToList());
    }
}
