import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import UploadPage from './pages/UploadPage';
import BookDetailPage from './pages/BookDetailPage';
import ChatPage from './pages/ChatPage';
import ReaderPage from './pages/ReaderPage';
import QuizPage from './pages/QuizPage';
import GraphPage from './pages/GraphPage';
import CrossBookPage from './pages/CrossBookPage';
import ProfilePage from './pages/ProfilePage';
import './App.css';

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/books/upload" element={<UploadPage />} />
            <Route path="/books/:id" element={<BookDetailPage />} />
            <Route path="/books/:id/chat" element={<ChatPage />} />
            <Route path="/books/:id/read/:chapterId" element={<ReaderPage />} />
            <Route path="/books/:id/quiz/:chapterId" element={<QuizPage />} />
            <Route path="/books/:id/graph" element={<GraphPage />} />
            <Route path="/cross-book" element={<CrossBookPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
