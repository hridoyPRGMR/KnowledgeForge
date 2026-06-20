using System.Net.Http.Headers;
using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Infrastructure.Ai.Ollama;
using KnowledgeForge.Infrastructure.Ai.OpenAiCompatible;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeForge.Infrastructure.Ai;

public static class AiProviderRegistration
{
    public static IServiceCollection AddAiProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatOptions>(configuration.GetSection(ChatOptions.SectionName));
        services.Configure<EmbeddingOptions>(configuration.GetSection(EmbeddingOptions.SectionName));

        AddChatProvider(services, configuration);
        AddEmbeddingProvider(services, configuration);

        return services;
    }

    private static void AddChatProvider(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ChatOptions.SectionName).Get<ChatOptions>() ?? new ChatOptions();

        switch (options.Provider)
        {
            case AiProvider.Ollama:
                services.AddHttpClient<OllamaChatClient>(client => ConfigureHttpClient(client, options.BaseUrl, options.ApiKey));
                services.AddScoped<IChatCompletionService>(sp => sp.GetRequiredService<OllamaChatClient>());
                break;
            case AiProvider.OpenAiCompatible:
                services.AddHttpClient<OpenAiCompatibleChatClient>(client => ConfigureHttpClient(client, options.BaseUrl, options.ApiKey));
                services.AddScoped<IChatCompletionService>(sp => sp.GetRequiredService<OpenAiCompatibleChatClient>());
                break;
            default:
                throw new InvalidOperationException($"Unknown chat provider: {options.Provider}");
        }
    }

    private static void AddEmbeddingProvider(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(EmbeddingOptions.SectionName).Get<EmbeddingOptions>() ?? new EmbeddingOptions();

        switch (options.Provider)
        {
            case AiProvider.Ollama:
                services.AddHttpClient<OllamaEmbeddingClient>(client => ConfigureHttpClient(client, options.BaseUrl, options.ApiKey));
                services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<OllamaEmbeddingClient>());
                break;
            case AiProvider.OpenAiCompatible:
                services.AddHttpClient<OpenAiCompatibleEmbeddingClient>(client => ConfigureHttpClient(client, options.BaseUrl, options.ApiKey));
                services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<OpenAiCompatibleEmbeddingClient>());
                break;
            default:
                throw new InvalidOperationException($"Unknown embedding provider: {options.Provider}");
        }
    }

    private static void ConfigureHttpClient(HttpClient client, string baseUrl, string apiKey)
    {
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
    }
}
