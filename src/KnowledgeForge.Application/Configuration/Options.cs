namespace KnowledgeForge.Application.Configuration;

public enum AiProvider
{
    Ollama,
    Llama,
    OpenAiCompatible
}

public class ChatOptions
{
    public const string SectionName = "Chat";
    public AiProvider Provider { get; set; } = AiProvider.Ollama;
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen2.5:3b";
    public string ApiKey { get; set; } = string.Empty;
}

public class EmbeddingOptions
{
    public const string SectionName = "Embeddings";
    public AiProvider Provider { get; set; } = AiProvider.Ollama;
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "nomic-embed-text";
    public string ApiKey { get; set; } = string.Empty;
}

public class RagOptions
{
    public const string SectionName = "Rag";
    public int TopK { get; set; } = 5;
    public int ChunkSize { get; set; } = 500;
    public int ChunkOverlap { get; set; } = 50;
}

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string BookPath { get; set; } = "./data/books";
}

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
