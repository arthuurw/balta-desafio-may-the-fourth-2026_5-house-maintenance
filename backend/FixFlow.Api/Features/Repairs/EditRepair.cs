using FixFlow.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Repairs;

static class EditRepair
{
    public record Request(Guid Id, string? Description, string? Category);

    internal sealed class Validator : AbstractValidator<Request>
    {
        private static readonly string[] ValidCategories =
            ["eletrico", "hidraulico", "fixacao", "limpeza", "geral"];

        public Validator()
        {
            RuleFor(x => x).Must(x => x.Description is not null || x.Category is not null)
                .WithMessage("At least one of Description or Category must be provided.");
            RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
            RuleFor(x => x.Category).Must(c => ValidCategories.Contains(c))
                .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.")
                .When(x => x.Category is not null);
        }
    }

    public static async Task<Results<Ok<RepairDto>, BadRequest<string[]>, NotFound>> HandleAsync(
        Guid id,
        Request req,
        IValidator<Request> validator,
        FixFlowContext db,
        CancellationToken ct)
    {
        var fullReq = req with { Id = id };
        var v = await validator.ValidateAsync(fullReq, ct);
        if (!v.IsValid)
            return TypedResults.BadRequest(v.Errors.Select(e => e.ErrorMessage).ToArray());

        var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (repair is null) return TypedResults.NotFound();

        if (req.Description is not null) repair.Description = req.Description;
        if (req.Category is not null) repair.Category = req.Category;
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(repair.ToDto());
    }
}
