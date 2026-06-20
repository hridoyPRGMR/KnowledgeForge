using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using Microsoft.Extensions.Options;
using Pgvector;

namespace KnowledgeForge.Infrastructure.Ai.OpenAiCompatible;

public class OpenAiCompatibleEmbeddingClient(HttpClient httpClient, IOptions<EmbeddingOptions> options) : IEmbeddingService
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

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty embedding response from OpenAI-compatible provider.");

        var embedding = result.Data?.FirstOrDefault()?.Embedding
            ?? throw new InvalidOperationException("Empty embedding data from OpenAI-compatible provider.");

        return new Vector(embedding);
    }

    private sealed record EmbeddingResponse(List<EmbeddingData>? Data);
    private sealed record EmbeddingData(float[] Embedding);
}
