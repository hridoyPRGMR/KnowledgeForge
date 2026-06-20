import { useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../api/client';

export default function ReaderPage() {
  const { id, chapterId } = useParams<{ id: string; chapterId: string }>();
  const contentRef = useRef<HTMLDivElement>(null);
  const [noteText, setNoteText] = useState('');
  const [selectedText, setSelectedText] = useState('');
  const queryClient = useQueryClient();

  const { data: content, isLoading } = useQuery({
    queryKey: ['chapter', id, chapterId],
    queryFn: () => api.getChapterContent(id!, chapterId!),
    enabled: !!id && !!chapterId,
  });

  const { data: notes } = useQuery({ queryKey: ['notes', id], queryFn: () => api.getNotes(id!), enabled: !!id });
  const { data: highlights } = useQuery({ queryKey: ['highlights', id], queryFn: () => api.getHighlights(id!), enabled: !!id });

  const createNote = useMutation({
    mutationFn: (text: string) => api.createNote(id!, text, chapterId),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['notes', id] }); setNoteText(''); },
  });

  const createHighlight = useMutation({
    mutationFn: (text: string) => {
      const fullText = content?.content ?? '';
      const start = fullText.indexOf(text);
      return api.createHighlight(id!, { chapterId, text, startOffset: start, endOffset: start + text.length, color: '#FFEB3B' });
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['highlights', id] }),
  });

  const generateSummary = useMutation({
    mutationFn: () => api.generateSummary(id!, chapterId!),
  });

  useEffect(() => {
    if (content && id) {
      api.updateProgress(id, {
        currentChapter: content.chapterNumber,
        currentPage: 1,
        percentComplete: Math.min(content.chapterNumber * 10, 100),
      }).catch(() => {});
    }
  }, [content, id]);

  const handleSelection = () => {
    const text = window.getSelection()?.toString().trim();
    if (text) setSelectedText(text);
  };

  if (isLoading) return <div className="loading">Loading chapter...</div>;
  if (!content) return <div className="error">Chapter not found</div>;

  const chapterNotes = notes?.filter((n) => n.chapterId === chapterId) ?? [];
  const chapterHighlights = highlights?.filter((h) => h.chapterId === chapterId) ?? [];

  return (
    <div className="reader-page">
      <Link to={`/books/${id}`} className="back-link">← Back to Book</Link>
      <div className="reader-header">
        <h1>{content.title}</h1>
        <button className="btn small" onClick={() => generateSummary.mutate()} disabled={generateSummary.isPending}>
          {generateSummary.isPending ? 'Summarizing...' : 'Summarize Chapter'}
        </button>
      </div>

      {generateSummary.data && (
        <div className="summary-panel">
          <h3>Summary</h3>
          <p>{generateSummary.data.summaryText}</p>
          <h4>Key Ideas</h4>
          <pre>{generateSummary.data.keyIdeas}</pre>
          <h4>Action Items</h4>
          <pre>{generateSummary.data.actionItems}</pre>
        </div>
      )}

      <div className="reader-layout">
        <div ref={contentRef} className="reader-content" onMouseUp={handleSelection}>
          {content.content}
        </div>

        <aside className="reader-sidebar">
          <h3>Highlight & Notes</h3>
          {selectedText && (
            <div className="selection-actions">
              <p className="selected-text">"{selectedText.slice(0, 80)}..."</p>
              <button className="btn small" onClick={() => createHighlight.mutate(selectedText)}>Highlight</button>
            </div>
          )}
          <textarea
            value={noteText}
            onChange={(e) => setNoteText(e.target.value)}
            placeholder="Add a note..."
            rows={3}
          />
          <button className="btn small" onClick={() => createNote.mutate(noteText)} disabled={!noteText.trim()}>Save Note</button>

          <h4>Notes ({chapterNotes.length})</h4>
          {chapterNotes.map((n) => <div key={n.id} className="sidebar-item">{n.content}</div>)}

          <h4>Highlights ({chapterHighlights.length})</h4>
          {chapterHighlights.map((h) => (
            <div key={h.id} className="sidebar-item highlight-item" style={{ borderLeftColor: h.color }}>
              {h.text}
            </div>
          ))}
        </aside>
      </div>
    </div>
  );
}
