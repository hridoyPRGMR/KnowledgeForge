using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Api.Endpoints;

public static class ChatEndpoints
{
    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat/{bookId:guid}", async (Guid bookId, ChatRequestDto request, IChatService chat, CancellationToken ct) =>
            Results.Ok(await chat.AskAsync(bookId, request, ct)));

        app.MapPost("/api/chat/cross-book", async (CrossBookChatRequestDto request, IChatService chat, CancellationToken ct) =>
            Results.Ok(await chat.AskCrossBookAsync(request, ct)));

        return app;
    }
}
