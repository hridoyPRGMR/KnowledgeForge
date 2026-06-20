using KnowledgeForge.Application.Interfaces;

namespace KnowledgeForge.Infrastructure.Ai;

public abstract class ChatCompletionClientBase : IChatCompletionService
{
    public abstract Task<string> GenerateAsync(string prompt, CancellationToken ct = default);

    public Task<string> GenerateJsonAsync(string prompt, CancellationToken ct = default)
    {
        var jsonPrompt = prompt + "\n\nRespond with valid JSON only. No markdown, no explanation.";
        return GenerateAsync(jsonPrompt, ct);
    }
}
