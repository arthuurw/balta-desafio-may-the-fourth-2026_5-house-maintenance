using FixFlow.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Repairs;

static class ReopenRepair
{
    public static async Task<Results<Ok<RepairDto>, NotFound>> HandleAsync(
        Guid id,
        FixFlowContext db,
        CancellationToken ct)
    {
        var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (repair is null) return TypedResults.NotFound();

        repair.Status = "pendente";
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(repair.ToDto());
    }
}
