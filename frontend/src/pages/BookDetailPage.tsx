import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';

export default function BookDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: book, isLoading, error } = useQuery({
    queryKey: ['book', id],
    queryFn: () => api.getBook(id!),
    enabled: !!id,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === 'Uploaded' || status === 'Processing' ? 3000 : false;
    },
  });

  if (isLoading) return <div className="loading">Loading book...</div>;
  if (error || !book) return <div className="error">Book not found</div>;

  return (
    <div>
      <Link to="/" className="back-link">← Back to Library</Link>
      <div className="page-header">
        <div>
          <h1>{book.title}</h1>
          <span className={`status status-${book.status.toLowerCase()}`}>{book.status}</span>
          {book.errorMessage && <p className="error">{book.errorMessage}</p>}
        </div>
        <div className="action-buttons">
          {book.status === 'Ready' && (
            <>
              <Link to={`/books/${id}/chat`} className="btn primary">Chat</Link>
              <Link to={`/books/${id}/graph`} className="btn">Knowledge Graph</Link>
            </>
          )}
        </div>
      </div>

      <h2>Chapters</h2>
      {book.chapters.length === 0 ? (
        <p className="empty-state">
          {book.status === 'Processing' ? 'Processing book... chapters will appear shortly.' : 'No chapters detected yet.'}
        </p>
      ) : (
        <div className="chapter-list">
          {book.chapters.map((chapter) => (
            <div key={chapter.id} className="chapter-item">
              <div>
                <strong>Chapter {chapter.chapterNumber}</strong>
                <p>{chapter.title}</p>
                <small>Pages {chapter.startPage}–{chapter.endPage}</small>
              </div>
              {book.status === 'Ready' && (
                <div className="chapter-actions">
                  <Link to={`/books/${id}/read/${chapter.id}`} className="btn small">Read</Link>
                  <Link to={`/books/${id}/quiz/${chapter.id}`} className="btn small">Quiz</Link>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
