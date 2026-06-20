import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { api } from '../api/client';

interface Message {
  role: 'user' | 'assistant';
  content: string;
  sources?: { chunkId: string; content: string; chunkIndex: number; chapterTitle: string }[];
}

export default function ChatPage() {
  const { id } = useParams<{ id: string }>();
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [conversationId, setConversationId] = useState<string>();

  const { data: book } = useQuery({ queryKey: ['book', id], queryFn: () => api.getBook(id!), enabled: !!id });

  const chat = useMutation({
    mutationFn: (message: string) => api.chat(id!, message, conversationId),
    onSuccess: (data) => {
      setConversationId(data.conversationId);
      setMessages((prev) => [
        ...prev,
        { role: 'assistant', content: data.answer, sources: data.sources },
      ]);
    },
  });

  const send = (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim()) return;
    setMessages((prev) => [...prev, { role: 'user', content: input }]);
    chat.mutate(input);
    setInput('');
  };

  return (
    <div className="chat-page">
      <Link to={`/books/${id}`} className="back-link">← Back to {book?.title ?? 'Book'}</Link>
      <h1>Chat with Book</h1>

      <div className="chat-messages">
        {messages.length === 0 && (
          <div className="empty-state">Ask a question about this book. Answers are grounded in the book content.</div>
        )}
        {messages.map((msg, i) => (
          <div key={i} className={`chat-bubble ${msg.role}`}>
            <p>{msg.content}</p>
            {msg.sources && msg.sources.length > 0 && (
              <details className="sources">
                <summary>Sources ({msg.sources.length})</summary>
                {msg.sources.map((s) => (
                  <blockquote key={s.chunkId}>
                    <small>{s.chapterTitle} · Chunk {s.chunkIndex}</small>
                    <p>{s.content.slice(0, 200)}...</p>
                  </blockquote>
                ))}
              </details>
            )}
          </div>
        ))}
        {chat.isPending && <div className="chat-bubble assistant loading">Thinking...</div>}
      </div>

      <form onSubmit={send} className="chat-input">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="What does the author say about..."
          disabled={chat.isPending}
        />
        <button type="submit" className="btn primary" disabled={chat.isPending}>Send</button>
      </form>
    </div>
  );
}
