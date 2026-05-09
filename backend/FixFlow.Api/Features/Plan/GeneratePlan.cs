using FixFlow.Api.Agents;
using FixFlow.Api.Data;
using FixFlow.Api.Features;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Plan;

public static class GeneratePlan
{
    public record Request(string SessionId);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator() => RuleFor(x => x.SessionId).NotEmpty().MaximumLength(128);
    }

    public static async Task<Results<Ok<Response>, BadRequest<string[]>, NotFound, StatusCodeHttpResult>> HandleAsync(
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

        var pending = repairs.Where(r => r.Status == "pendente").ToList();
        if (pending.Count == 0)
            return TypedResults.NotFound();

        var prompt = PromptBuilder.Build(req.SessionId, repairs,
            "Gere o plano de execução agora. Infira categorias e ferramentas para qualquer reparo sem elas e retorne ação generate_plan com todos os grupos preenchidos em planGroups.");

        try
        {
            var result = await agent.AskAsync(prompt, ct);
            return TypedResults.Ok(new Response(result.Reply, result.PlanGroups ?? []));
        }
        catch (AgentException)
        {
            return TypedResults.StatusCode(502);
        }
    }

    public record Response(string Reply, PlanGroupDto[] Groups);
}
