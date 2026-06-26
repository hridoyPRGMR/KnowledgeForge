import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { BookStatusName } from '../constants/bookStatus';

export default function Dashboard() {
  const { data: books, isLoading, error } = useQuery({ queryKey: ['books'], queryFn: api.getBooks });
  const { data: profile } = useQuery({ queryKey: ['profile'], queryFn: api.getProfile });

  if (isLoading) return <div className="loading">Loading library...</div>;
  if (error) return <div className="error">Failed to load books: {(error as Error).message}</div>;

  return (
    <div>
      <div className="page-header">
        <h1>KnowledgeForge</h1>
        <Link to="/books/upload" className="btn primary">Upload Book</Link>
      </div>

      {profile && (
        <div className="stats-grid">
          <div className="stat-card"><span>{profile.totalBooks}</span><label>Total Books</label></div>
          <div className="stat-card"><span>{profile.readyBooks}</span><label>Ready</label></div>
          <div className="stat-card"><span>{profile.chaptersRead}</span><label>Chapters Read</label></div>
          <div className="stat-card"><span>{Math.round(profile.averageProgress)}%</span><label>Avg Progress</label></div>
        </div>
      )}

      <h2>Your Library</h2>
      {books?.length === 0 ? (
        <div className="empty-state">
          <p>No books yet. Upload a PDF to get started.</p>
          <Link to="/books/upload" className="btn primary">Upload your first book</Link>
        </div>
      ) : (
        <div className="book-grid">
          {books?.map((book) => (
            <Link key={book.id} to={`/books/${book.id}`} className="book-card">
              <h3>{book.title}</h3>
              <span className={`status status-${BookStatusName[book.status]}`}>{BookStatusName[book.status]}</span>
              <p>{book.chapterCount} chapters · {book.pageCount} pages</p>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
