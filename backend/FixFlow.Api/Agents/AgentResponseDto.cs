using System.Text.Json;

namespace FixFlow.Api.Agents;

public sealed class AgentResponseDto
{
    public string Action { get; init; } = "unknown";
    public string Reply { get; init; } = string.Empty;
    public PlanGroupDto[]? PlanGroups { get; init; }
    public CreatedRepairDto[]? CreatedRepairs { get; init; }
    public EditedRepairDto? EditedRepair { get; init; }
    public string? MarkedDoneId { get; init; }
    public string? RemovedId { get; init; }
    public string? ReopenedId { get; init; }
    public RepairSummaryDto[]? Repairs { get; init; }
    public RepairSummaryDto? NextRepair { get; init; }
    public ShoppingKitDto[]? ShoppingList { get; init; }
    public TimeEstimateDto[]? TimeEstimates { get; init; }

    public static bool TryParse(string? json, out AgentResponseDto? dto)
    {
        dto = null;
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            dto = new AgentResponseDto
            {
                Action = root.TryGetStr("action") ?? "unknown",
                Reply = root.TryGetStr("reply") ?? string.Empty,
                PlanGroups = root.TryGetPlanGroups("planGroups"),
                CreatedRepairs = root.TryGetCreatedRepairs("createdRepairs"),
                EditedRepair = root.TryGetEditedRepair("editedRepair"),
                MarkedDoneId = root.TryGetStr("markedDoneId"),
                RemovedId = root.TryGetStr("removedId"),
                ReopenedId = root.TryGetStr("reopenedId"),
                Repairs = root.TryGetRepairSummaries("repairs"),
                NextRepair = root.TryGetRepairSummary("nextRepair"),
                ShoppingList = root.TryGetShoppingList("shoppingList"),
                TimeEstimates = root.TryGetTimeEstimates("timeEstimates"),
            };
            return true;
        }
        catch { return false; }
    }
}

public sealed record PlanGroupDto(int Group, string KitName, string[] Tools, string[] RepairIds);
public sealed record CreatedRepairDto(string Description, string? Category, string[] Tools);
public sealed record EditedRepairDto(string Id, string? Description, string? Category);
public sealed record RepairSummaryDto(string Id, string Description, string? Category, string[]? Tools);
public sealed record ShoppingKitDto(string KitName, string[] Items);
public sealed record TimeEstimateDto(string RepairId, string Description, int EstimatedMinutes);

static class JsonElementAgentExtensions
{
    public static string? TryGetStr(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    public static string[] TryGetStrArray(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return [];
        return arr.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString()!)
            .ToArray();
    }

    public static PlanGroupDto[]? TryGetPlanGroups(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        return arr.EnumerateArray().Select(g => new PlanGroupDto(
            g.TryGetProperty("group", out var gv) && gv.TryGetInt32(out var gi) ? gi : 0,
            g.TryGetStr("kitName") ?? string.Empty,
            g.TryGetStrArray("tools"),
            g.TryGetStrArray("repairIds")
        )).ToArray();
    }

    public static CreatedRepairDto[]? TryGetCreatedRepairs(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        return arr.EnumerateArray().Select(r => new CreatedRepairDto(
            r.TryGetStr("description") ?? string.Empty,
            r.TryGetStr("category"),
            r.TryGetStrArray("tools")
        )).ToArray();
    }

    public static EditedRepairDto? TryGetEditedRepair(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var obj) || obj.ValueKind != JsonValueKind.Object)
            return null;
        return new EditedRepairDto(
            obj.TryGetStr("id") ?? string.Empty,
            obj.TryGetStr("description"),
            obj.TryGetStr("category")
        );
    }

    public static RepairSummaryDto[]? TryGetRepairSummaries(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        return arr.EnumerateArray().Select(ToRepairSummary).ToArray();
    }

    public static RepairSummaryDto? TryGetRepairSummary(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var obj) || obj.ValueKind != JsonValueKind.Object)
            return null;
        return obj.ToRepairSummary();
    }

    public static RepairSummaryDto ToRepairSummary(this JsonElement r)
        => new(
            r.TryGetStr("id") ?? string.Empty,
            r.TryGetStr("description") ?? string.Empty,
            r.TryGetStr("category"),
            r.TryGetProperty("tools", out var tv) && tv.ValueKind == JsonValueKind.Array
                ? tv.EnumerateArray().Select(t => t.GetString()!).ToArray()
                : null
        );

    public static ShoppingKitDto[]? TryGetShoppingList(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        return arr.EnumerateArray().Select(k => new ShoppingKitDto(
            k.TryGetStr("kitName") ?? string.Empty,
            k.TryGetStrArray("items")
        )).ToArray();
    }

    public static TimeEstimateDto[]? TryGetTimeEstimates(this JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        return arr.EnumerateArray().Select(t => new TimeEstimateDto(
            t.TryGetStr("repairId") ?? string.Empty,
            t.TryGetStr("description") ?? string.Empty,
            t.TryGetProperty("estimatedMinutes", out var mv) && mv.TryGetInt32(out var mi) ? mi : 0
        )).ToArray();
    }
}
