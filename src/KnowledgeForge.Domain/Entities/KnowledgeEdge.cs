namespace KnowledgeForge.Domain.Entities;

public class KnowledgeEdge
{
    public Guid Id { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public double Weight { get; set; } = 1.0;

    public KnowledgeNode SourceNode { get; set; } = null!;
    public KnowledgeNode TargetNode { get; set; } = null!;
}
