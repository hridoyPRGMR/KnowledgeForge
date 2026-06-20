# Seed a sample book for demos

1. Start the platform (see README).
2. Upload any PDF via the UI at http://localhost:5173/books/upload
   or via curl:

```bash
curl -X POST http://localhost:8080/api/books/upload \
  -F "file=@sample.pdf"
```

3. Poll status until Ready:

```bash
curl http://localhost:8080/api/books/{bookId}/status
```

4. Try chat:

```bash
curl -X POST http://localhost:8080/api/chat/{bookId} \
  -H "Content-Type: application/json" \
  -d '{"message": "What is this book about?"}'
```

Place a `sample.pdf` in this folder for quick testing.
