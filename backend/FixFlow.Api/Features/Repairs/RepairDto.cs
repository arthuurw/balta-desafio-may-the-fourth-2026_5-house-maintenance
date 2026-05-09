using FixFlow.Api.Data.Entities;

namespace FixFlow.Api.Features.Repairs;

public record RepairDto(
    Guid Id,
    string SessionId,
    string Description,
    string? Category,
    string[] Tools,
    string Status,
    DateTime CreatedAt);

public static class RepairMappings
{
    public static RepairDto ToDto(this Repair r)
        => new(r.Id, r.SessionId, r.Description, r.Category, r.Tools(), r.Status, r.CreatedAt);
}
