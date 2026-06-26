using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeForge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace KnowledgeForge.Infrastructure.Ai.Ollama;

public class OllamaChatClient(HttpClient httpClient, IOptions<ChatOptions> options) : ChatCompletionClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ChatOptions _options = options.Value;

    public override async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var request = new
        {
            model = _options.Model,
            messages = new[] { new { role = "user", content = prompt } },
            stream = false
        };

        var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty chat response from Ollama.");

        return result.Message?.Content ?? string.Empty;
    }

    private sealed record ChatResponse(ChatMessage? Message);
    private sealed record ChatMessage(string? Content);
}
