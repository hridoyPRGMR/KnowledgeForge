import { useEffect, useRef } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import ForceGraph2D from 'react-force-graph-2d';
import { api } from '../api/client';

export default function GraphPage() {
  const { id } = useParams<{ id: string }>();
  const containerRef = useRef<HTMLDivElement>(null);
  const graphRef = useRef<any>(null);

  const { data: graph, isLoading, error } = useQuery({
    queryKey: ['graph', id],
    queryFn: () => api.getGraph(id!),
    enabled: !!id,
    retry: 3,
    retryDelay: 5000,
  });

  const { data: book } = useQuery({ queryKey: ['book', id], queryFn: () => api.getBook(id!), enabled: !!id });

  useEffect(() => {
    if (containerRef.current && graphRef.current) {
      graphRef.current.width(containerRef.current.clientWidth);
    }
  }, [graph]);

  if (isLoading) return <div className="loading">Loading knowledge graph...</div>;
  if (error || !graph) return <div className="empty-state">Knowledge graph not available yet. It may still be extracting.</div>;

  const graphData = {
    nodes: graph.nodes.map((n) => ({ id: n.id, name: n.label, type: n.type })),
    links: graph.edges.map((e) => ({ source: e.sourceId, target: e.targetId, label: e.relationType })),
  };

  return (
    <div>
      <Link to={`/books/${id}`} className="back-link">← Back to {book?.title ?? 'Book'}</Link>
      <h1>Knowledge Graph</h1>
      <p>{graph.nodes.length} concepts · {graph.edges.length} relationships</p>
      <div ref={containerRef} className="graph-container">
        <ForceGraph2D
          ref={graphRef}
          graphData={graphData}
          nodeLabel="name"
          linkLabel="label"
          nodeCanvasObject={(node: any, ctx, globalScale) => {
            const label = node.name;
            const fontSize = 12 / globalScale;
            ctx.font = `${fontSize}px Sans-Serif`;
            ctx.fillStyle = '#6366f1';
            ctx.beginPath();
            ctx.arc(node.x, node.y, 4, 0, 2 * Math.PI);
            ctx.fill();
            ctx.fillStyle = '#1e293b';
            ctx.fillText(label, node.x + 6, node.y + 3);
          }}
          height={500}
        />
      </div>
    </div>
  );
}
