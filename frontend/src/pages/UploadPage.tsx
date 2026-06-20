import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { api } from '../api/client';

export default function UploadPage() {
  const [file, setFile] = useState<File | null>(null);
  const navigate = useNavigate();

  const upload = useMutation({
    mutationFn: (f: File) => api.uploadBook(f),
    onSuccess: (book) => navigate(`/books/${book.id}`),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (file) upload.mutate(file);
  };

  return (
    <div className="upload-page">
      <h1>Upload Book</h1>
      <p className="subtitle">Upload a PDF to extract knowledge, enable chat, summaries, and quizzes.</p>

      <form onSubmit={handleSubmit} className="upload-form">
        <div className="drop-zone">
          <input
            type="file"
            accept=".pdf"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
          {file && <p>Selected: {file.name}</p>}
        </div>

        {upload.error && <div className="error">{(upload.error as Error).message}</div>}

        <button type="submit" className="btn primary" disabled={!file || upload.isPending}>
          {upload.isPending ? 'Uploading...' : 'Upload & Process'}
        </button>
      </form>
    </div>
  );
}
