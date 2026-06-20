using KnowledgeForge.Domain.Enums;

namespace KnowledgeForge.Domain.Entities;

public class KnowledgeNode
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string NormalizedLabel { get; set; } = string.Empty;
    public KnowledgeNodeType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    public Book Book { get; set; } = null!;
    public ICollection<KnowledgeEdge> OutgoingEdges { get; set; } = [];
    public ICollection<KnowledgeEdge> IncomingEdges { get; set; } = [];
}
