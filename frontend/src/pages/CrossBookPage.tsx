import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { api } from '../api/client';

export default function CrossBookPage() {
  const [selectedBooks, setSelectedBooks] = useState<string[]>([]);
  const [input, setInput] = useState('');
  const [answer, setAnswer] = useState<string | null>(null);

  const { data: books } = useQuery({ queryKey: ['books'], queryFn: api.getBooks });
  const readyBooks = books?.filter((b) => b.status === 'Ready') ?? [];

  const chat = useMutation({
    mutationFn: () => api.crossBookChat(selectedBooks, input),
    onSuccess: (data) => setAnswer(data.answer),
  });

  const toggleBook = (bookId: string) => {
    setSelectedBooks((prev) =>
      prev.includes(bookId) ? prev.filter((id) => id !== bookId) : [...prev, bookId]
    );
  };

  const send = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedBooks.length === 0 || !input.trim()) return;
    chat.mutate();
  };

  return (
    <div className="cross-book-page">
      <h1>Cross-Book Reasoning</h1>
      <p className="subtitle">Ask questions across multiple books and discover connections.</p>

      <h3>Select Books</h3>
      <div className="book-select-grid">
        {readyBooks.map((book) => (
          <label key={book.id} className={`book-select ${selectedBooks.includes(book.id) ? 'selected' : ''}`}>
            <input type="checkbox" checked={selectedBooks.includes(book.id)} onChange={() => toggleBook(book.id)} />
            {book.title}
          </label>
        ))}
      </div>

      <form onSubmit={send} className="chat-input">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="How do these books relate on the topic of..."
          disabled={chat.isPending}
        />
        <button type="submit" className="btn primary" disabled={chat.isPending || selectedBooks.length === 0}>
          Ask
        </button>
      </form>

      {chat.isPending && <div className="loading">Reasoning across books...</div>}
      {answer && (
        <div className="chat-bubble assistant">
          <p>{answer}</p>
        </div>
      )}
      {chat.data?.sources && (
        <details className="sources">
          <summary>Sources ({chat.data.sources.length})</summary>
          {chat.data.sources.map((s) => (
            <blockquote key={s.chunkId}>
              <small>{s.bookTitle} · {s.chapterTitle}</small>
              <p>{s.content.slice(0, 200)}...</p>
            </blockquote>
          ))}
        </details>
      )}
    </div>
  );
}
