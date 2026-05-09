using FixFlow.Api.Data;
using FixFlow.Api.Data.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FixFlow.Api.Features.Repairs;

static class CreateRepair
{
    public record Request(string SessionId, string Description);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SessionId).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }

    public static async Task<Results<Created<RepairDto>, BadRequest<string[]>>> HandleAsync(
        Request req,
        IValidator<Request> validator,
        FixFlowContext db,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(req, ct);
        if (!v.IsValid)
            return TypedResults.BadRequest(v.Errors.Select(e => e.ErrorMessage).ToArray());

        var repair = new Repair
        {
            Id = Guid.NewGuid(),
            SessionId = req.SessionId,
            Description = req.Description,
            Status = "pendente",
            CreatedAt = DateTime.UtcNow
        };

        db.Repairs.Add(repair);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/repairs/{repair.Id}", repair.ToDto());
    }
}
