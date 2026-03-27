namespace MediConnect.Application.Configurations;

public class RagSettings
{
    public QdrantSettings Qdrant { get; set; } = new();
    public GroqSettings Groq { get; set; } = new();
    public OllamaSettings Ollama { get; set; } = new();
    public EmbeddingSettings Embedding { get; set; } = new();
    public string UseLLM { get; set; } = "Groq";
}

public class QdrantSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "mediconnect_knowledge";
    public int VectorSize { get; set; } = 384;
}

public class GroqSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
}

public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.1:8b";
}

public class EmbeddingSettings
{
    public string ModelPath { get; set; } = "./Models/all-MiniLM-L6-v2";
    public int Dimension { get; set; } = 384;
}
