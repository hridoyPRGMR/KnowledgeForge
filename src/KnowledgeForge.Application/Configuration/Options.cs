namespace KnowledgeForge.Application.Configuration;

public class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "qwen2.5:3b";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
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
