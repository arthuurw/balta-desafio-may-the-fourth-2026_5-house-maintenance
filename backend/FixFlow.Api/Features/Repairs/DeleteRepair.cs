using FixFlow.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Features.Repairs;

static class DeleteRepair
{
    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        FixFlowContext db,
        CancellationToken ct)
    {
        var repair = await db.Repairs.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (repair is null) return TypedResults.NotFound();

        db.Repairs.Remove(repair);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
