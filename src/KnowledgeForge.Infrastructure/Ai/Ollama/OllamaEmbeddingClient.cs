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
        var request = new { model = _options.Model, input = text };
        var response = await httpClient.PostAsJsonAsync("/v1/embeddings", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

       var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty embedding response.");

        if (result.Data.Length == 0)
            throw new InvalidOperationException("No embedding returned.");

        if (result.Data[0].Embedding.Length == 0)
            throw new InvalidOperationException("Embedding is empty.");

        return new Vector(result.Data[0].Embedding);
    }

    private sealed record EmbeddingResponse(
        [property: JsonPropertyName("data")] DataItem[] Data);

    private sealed record DataItem(
        [property: JsonPropertyName("embedding")] float[] Embedding);
}
