using FixFlow.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Repairs;

public static class ListRepairs
{
    public record Request(string SessionId);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator() => RuleFor(x => x.SessionId).NotEmpty().MaximumLength(128);
    }

    public static async Task<Results<Ok<Response>, BadRequest<string[]>>> HandleAsync(
        [AsParameters] Request req,
        IValidator<Request> validator,
        FixFlowContext db,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(req, ct);
        if (!v.IsValid)
            return TypedResults.BadRequest(v.Errors.Select(e => e.ErrorMessage).ToArray());

        var repairs = await db.Repairs
            .Where(r => r.SessionId == req.SessionId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => r.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new Response(repairs));
    }

    public record Response(List<RepairDto> Repairs);
}
