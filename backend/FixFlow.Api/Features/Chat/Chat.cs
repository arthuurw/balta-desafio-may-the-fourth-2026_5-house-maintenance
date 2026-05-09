using System.Text.Json;
using FixFlow.Api.Agents;
using FixFlow.Api.Data;
using FixFlow.Api.Data.Entities;
using FixFlow.Api.Features.Repairs;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Chat;

public static class Chat
{
    public record Request(string SessionId, string Message);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SessionId).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        }
    }

    public static async Task<Results<Ok<Response>, BadRequest<string[]>, StatusCodeHttpResult>> HandleAsync(
        Request req,
        IValidator<Request> validator,
        IRepairAgent agent,
        FixFlowContext db,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(req, ct);
        if (!v.IsValid)
            return TypedResults.BadRequest(v.Errors.Select(e => e.ErrorMessage).ToArray());

        var repairs = await db.Repairs
            .Where(r => r.SessionId == req.SessionId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        var prompt = PromptBuilder.Build(req.SessionId, repairs, req.Message);

        AgentResponseDto agentResult;
        try
        {
            agentResult = await agent.AskAsync(prompt, ct);
        }
        catch (AgentException)
        {
            return TypedResults.StatusCode(502);
        }

        var response = await ApplyActionAsync(agentResult, req.SessionId, db, ct);
        return TypedResults.Ok(response);
    }

    private static async Task<Response> ApplyActionAsync(
        AgentResponseDto agentResult, string sessionId, FixFlowContext db, CancellationToken ct)
    {
        switch (agentResult.Action)
        {
            case "add_repair" or "bulk_add" when agentResult.CreatedRepairs is { Length: > 0 } crs:
            {
                var created = new List<RepairDto>();
                foreach (var cr in crs)
                {
                    var repair = new Repair
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionId,
                        Description = cr.Description,
                        Category = cr.Category,
                        ToolsJson = JsonSerializer.Serialize(cr.Tools),
                        Status = "pendente",
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Repairs.Add(repair);
                    created.Add(repair.ToDto());
                }
                await db.SaveChangesAsync(ct);
                return new Response(agentResult.Action, agentResult.Reply, CreatedRepairs: [.. created]);
            }

            case "edit_repair" when agentResult.EditedRepair is { } er
                && Guid.TryParse(er.Id, out var eid):
            {
                var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == eid, ct);
                if (repair is not null)
                {
                    if (er.Description is not null) repair.Description = er.Description;
                    if (er.Category is not null) repair.Category = er.Category;
                    await db.SaveChangesAsync(ct);
                    return new Response(agentResult.Action, agentResult.Reply, EditedRepair: repair.ToDto());
                }
                return new Response(agentResult.Action, agentResult.Reply);
            }

            case "mark_done" when agentResult.MarkedDoneId is { } rawId
                && Guid.TryParse(rawId, out var id):
            {
                var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
                if (repair is not null)
                {
                    repair.Status = "concluido";
                    await db.SaveChangesAsync(ct);
                }
                return new Response(agentResult.Action, agentResult.Reply, MarkedDoneId: rawId);
            }

            case "undo_done" when agentResult.ReopenedId is { } rawId
                && Guid.TryParse(rawId, out var id):
            {
                var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
                if (repair is not null)
                {
                    repair.Status = "pendente";
                    await db.SaveChangesAsync(ct);
                }
                return new Response(agentResult.Action, agentResult.Reply, ReopenedId: rawId);
            }

            case "remove_repair" when agentResult.RemovedId is { } rawId
                && Guid.TryParse(rawId, out var id):
            {
                var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
                if (repair is not null)
                {
                    db.Repairs.Remove(repair);
                    await db.SaveChangesAsync(ct);
                }
                return new Response(agentResult.Action, agentResult.Reply, RemovedId: rawId);
            }

            default:
                return new Response(
                    agentResult.Action,
                    agentResult.Reply,
                    PlanGroups: agentResult.PlanGroups,
                    Repairs: agentResult.Repairs,
                    NextRepair: agentResult.NextRepair,
                    ShoppingList: agentResult.ShoppingList,
                    TimeEstimates: agentResult.TimeEstimates);
        }
    }

    public record Response(
        string Action,
        string Reply,
        RepairDto[]? CreatedRepairs = null,
        RepairDto? EditedRepair = null,
        string? MarkedDoneId = null,
        string? ReopenedId = null,
        string? RemovedId = null,
        PlanGroupDto[]? PlanGroups = null,
        RepairSummaryDto[]? Repairs = null,
        RepairSummaryDto? NextRepair = null,
        ShoppingKitDto[]? ShoppingList = null,
        TimeEstimateDto[]? TimeEstimates = null);
}
