using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using Microsoft.Extensions.Options;
using Pgvector;

namespace KnowledgeForge.Infrastructure.Ai.Ollama;

public class OllamaEmbeddingClient(HttpClient httpClient, IOptions<EmbeddingOptions> options) : IEmbeddingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly EmbeddingOptions _options = options.Value;

    public async Task<Vector> GenerateAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = _options.Model, prompt = text };
        var response = await httpClient.PostAsJsonAsync("/api/embeddings", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty embedding response from Ollama.");

        return new Vector(result.Embedding);
    }

    private sealed record EmbeddingResponse(float[] Embedding);
}
