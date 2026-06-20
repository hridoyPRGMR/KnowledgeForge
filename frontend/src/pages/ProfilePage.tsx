import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { api } from '../api/client';

export default function ProfilePage() {
  const { data: profile, isLoading } = useQuery({ queryKey: ['profile'], queryFn: api.getProfile });

  if (isLoading) return <div className="loading">Loading profile...</div>;
  if (!profile) return <div className="error">Profile unavailable</div>;

  return (
    <div>
      <h1>Learning Profile</h1>
      <div className="stats-grid">
        <div className="stat-card"><span>{profile.totalBooks}</span><label>Books</label></div>
        <div className="stat-card"><span>{profile.readyBooks}</span><label>Ready</label></div>
        <div className="stat-card"><span>{profile.chaptersRead}</span><label>Chapters Read</label></div>
        <div className="stat-card"><span>{Math.round(profile.averageProgress)}%</span><label>Progress</label></div>
      </div>

      <h2>Recently Read</h2>
      {profile.recentBooks.length === 0 ? (
        <p className="empty-state">Start reading a book to track your progress.</p>
      ) : (
        <div className="progress-list">
          {profile.recentBooks.map((p) => (
            <Link key={p.bookId} to={`/books/${p.bookId}`} className="progress-item">
              <div>
                <strong>{p.bookTitle}</strong>
                <p>Chapter {p.currentChapter} · {Math.round(p.percentComplete)}% complete</p>
              </div>
              <div className="progress-bar">
                <div className="progress-fill" style={{ width: `${p.percentComplete}%` }} />
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
