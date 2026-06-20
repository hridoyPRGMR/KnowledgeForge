# KnowledgeForge

An AI-powered knowledge platform that transforms books into searchable, conversational knowledge.

## Architecture

- **ASP.NET Core API** — REST endpoints for books, chat, summaries, quizzes, notes
- **Worker Service** — Async PDF processing, embedding generation, knowledge graph extraction
- **PostgreSQL + pgvector** — Relational data and vector search
- **Redis** — Chat answer caching
- **RabbitMQ** — Async book processing queue
- **Ollama** — Local LLM (Qwen 2.5 3B) and embeddings (nomic-embed-text)
- **React** — Frontend SPA

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 22+](https://nodejs.org/) (for local frontend dev)

## Quick Start (Docker)

```bash
# Start infrastructure
docker compose up -d postgres redis rabbitmq ollama

# Pull Ollama models (first time only)
docker exec -it knowledgeforge-ollama-1 ollama pull qwen2.5:3b
docker exec -it knowledgeforge-ollama-1 ollama pull nomic-embed-text

# Run API and Worker locally (or use docker compose up)
dotnet run --project src/KnowledgeForge.Api
dotnet run --project src/KnowledgeForge.Worker

# Frontend
cd frontend && npm install && npm run dev
```

Or run everything with Docker:

```bash
docker compose up --build
```

- Frontend: http://localhost:3000
- API: http://localhost:8080
- API Health: http://localhost:8080/api/health
- RabbitMQ Management: http://localhost:15672 (guest/guest)

## Local Development

### Backend

```bash
dotnet restore
dotnet build
dotnet ef database update --project src/KnowledgeForge.Infrastructure --startup-project src/KnowledgeForge.Api
dotnet run --project src/KnowledgeForge.Api
dotnet run --project src/KnowledgeForge.Worker
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Set `VITE_API_URL=http://localhost:8080` in `frontend/.env` if needed.

## Features

| Feature | Description |
|---------|-------------|
| Book Upload | PDF upload with async processing |
| RAG Chat | Ask questions grounded in book content |
| Summaries | AI-generated chapter summaries with key ideas |
| Quizzes | MCQ, flashcards, true/false generation |
| Notes & Highlights | Kindle-style annotations |
| Knowledge Graph | Concept extraction and visualization |
| Cross-Book Reasoning | Query across multiple books |
| Learning Profile | Reading progress tracking |

## Project Structure

```
src/
├── KnowledgeForge.Api/           # HTTP API
├── KnowledgeForge.Application/     # Interfaces, DTOs, config
├── KnowledgeForge.Domain/          # Entities, enums, events
├── KnowledgeForge.Infrastructure/  # EF Core, services, integrations
└── KnowledgeForge.Worker/          # Background processing
frontend/                           # React SPA
```

## Configuration

See `src/KnowledgeForge.Api/appsettings.json` for connection strings and Ollama settings.

## License

MIT
