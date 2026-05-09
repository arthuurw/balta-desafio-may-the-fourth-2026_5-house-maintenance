using System.Text.Json;

namespace FixFlow.Api.Data.Entities;

public sealed class Repair
{
    public Guid Id { get; set; }
    public required string SessionId { get; set; }
    public required string Description { get; set; }
    public string? Category { get; set; }
    public string ToolsJson { get; set; } = "[]";
    public string Status { get; set; } = "pendente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string[] Tools()
        => JsonSerializer.Deserialize<string[]>(ToolsJson) ?? [];
}
