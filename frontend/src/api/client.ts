export const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

export interface Book {
  id: string;
  title: string;
  status: 'Uploaded' | 'Processing' | 'Ready' | 'Failed';
  pageCount: number;
  createdAt: string;
  chapterCount: number;
}

export interface Chapter {
  id: string;
  chapterNumber: number;
  title: string;
  startPage: number;
  endPage: number;
}

export interface BookDetail extends Omit<Book, 'chapterCount'> {
  errorMessage?: string;
  chapters: Chapter[];
}

export interface BookStatus {
  id: string;
  status: Book['status'];
  errorMessage?: string;
  chunkCount: number;
}

export interface ChatResponse {
  conversationId: string;
  answer: string;
  sources: { chunkId: string; content: string; chunkIndex: number; chapterTitle: string }[];
}

export interface Summary {
  id: string;
  summaryText: string;
  keyIdeas: string;
  actionItems: string;
}

export interface QuizQuestion {
  id: string;
  question: string;
  options: string[];
  correctAnswer: string;
  explanation: string;
}

export interface Quiz {
  id: string;
  type: string;
  questions: QuizQuestion[];
}

export interface Note {
  id: string;
  bookId: string;
  chapterId?: string;
  content: string;
  createdAt: string;
}

export interface Highlight {
  id: string;
  bookId: string;
  chapterId?: string;
  text: string;
  startOffset: number;
  endOffset: number;
  color: string;
  createdAt: string;
}

export interface ReadingProgress {
  bookId: string;
  bookTitle: string;
  currentChapter: number;
  currentPage: number;
  percentComplete: number;
  lastReadAt: string;
}

export interface Profile {
  totalBooks: number;
  readyBooks: number;
  chaptersRead: number;
  averageProgress: number;
  recentBooks: ReadingProgress[];
}

export interface KnowledgeGraph {
  nodes: { id: string; label: string; type: string; description: string }[];
  edges: { id: string; sourceId: string; targetId: string; relationType: string; weight: number }[];
}

export interface CrossBookResponse {
  answer: string;
  sources: { bookId: string; bookTitle: string; chunkId: string; content: string; chapterTitle: string }[];
}

export interface ChapterContent {
  id: string;
  title: string;
  chapterNumber: number;
  content: string;
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, options);
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || res.statusText);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const api = {
  getBooks: () => request<Book[]>('/api/books'),
  getBook: (id: string) => request<BookDetail>(`/api/books/${id}`),
  getBookStatus: (id: string) => request<BookStatus>(`/api/books/${id}/status`),
  uploadBook: async (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return request<Book>('/api/books/upload', { method: 'POST', body: form });
  },
  getChapterContent: (bookId: string, chapterId: string) =>
    request<ChapterContent>(`/api/books/${bookId}/chapters/${chapterId}/content`),
  chat: (bookId: string, message: string, conversationId?: string) =>
    request<ChatResponse>(`/api/chat/${bookId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ conversationId, message }),
    }),
  crossBookChat: (bookIds: string[], message: string) =>
    request<CrossBookResponse>('/api/chat/cross-book', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ bookIds, message }),
    }),
  getSummary: (bookId: string, chapterId: string) =>
    request<Summary>(`/api/books/${bookId}/chapters/${chapterId}/summary`),
  generateSummary: (bookId: string, chapterId: string) =>
    request<Summary>(`/api/books/${bookId}/chapters/${chapterId}/summarize`, { method: 'POST' }),
  getQuiz: (bookId: string, chapterId: string, type: string) =>
    request<Quiz>(`/api/books/${bookId}/chapters/${chapterId}/quiz/${type}`),
  generateQuiz: (bookId: string, chapterId: string, type: string) =>
    request<Quiz>(`/api/books/${bookId}/chapters/${chapterId}/quiz?type=${type}`, { method: 'POST' }),
  getNotes: (bookId: string) => request<Note[]>(`/api/books/${bookId}/notes`),
  createNote: (bookId: string, content: string, chapterId?: string) =>
    request<Note>(`/api/books/${bookId}/notes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ chapterId, content }),
    }),
  deleteNote: (id: string) => request<void>(`/api/notes/${id}`, { method: 'DELETE' }),
  getHighlights: (bookId: string) => request<Highlight[]>(`/api/books/${bookId}/highlights`),
  createHighlight: (bookId: string, data: Omit<Highlight, 'id' | 'bookId' | 'createdAt'>) =>
    request<Highlight>(`/api/books/${bookId}/highlights`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }),
  deleteHighlight: (id: string) => request<void>(`/api/highlights/${id}`, { method: 'DELETE' }),
  getProgress: (bookId: string) => request<ReadingProgress>(`/api/books/${bookId}/progress`),
  updateProgress: (bookId: string, data: { currentChapter: number; currentPage: number; percentComplete: number }) =>
    request<ReadingProgress>(`/api/books/${bookId}/progress`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }),
  getProfile: () => request<Profile>('/api/profile'),
  getGraph: (bookId: string) => request<KnowledgeGraph>(`/api/books/${bookId}/graph`),
};
