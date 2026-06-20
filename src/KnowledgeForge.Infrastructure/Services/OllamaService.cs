using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using Microsoft.Extensions.Options;
using Pgvector;

namespace KnowledgeForge.Infrastructure.Services;

public class OllamaService(HttpClient httpClient, IOptions<OllamaOptions> options) : IOllamaService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly OllamaOptions _options = options.Value;

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = _options.EmbeddingModel, prompt = text };
        var response = await httpClient.PostAsJsonAsync("/api/embeddings", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty embedding response from Ollama.");

        return new Vector(result.Embedding);
    }

    public async Task<string> GenerateChatAsync(string prompt, CancellationToken ct = default)
    {
        var request = new
        {
            model = _options.ChatModel,
            messages = new[] { new { role = "user", content = prompt } },
            stream = false
        };

        var response = await httpClient.PostAsJsonAsync("/api/chat", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty chat response from Ollama.");

        return result.Message?.Content ?? string.Empty;
    }

    public async Task<string> GenerateJsonAsync(string prompt, CancellationToken ct = default)
    {
        var jsonPrompt = prompt + "\n\nRespond with valid JSON only. No markdown, no explanation.";
        return await GenerateChatAsync(jsonPrompt, ct);
    }

    private sealed record EmbeddingResponse(float[] Embedding);
    private sealed record ChatResponse(ChatMessage? Message);
    private sealed record ChatMessage(string? Content);
}
