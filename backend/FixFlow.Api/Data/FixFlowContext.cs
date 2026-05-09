using FixFlow.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Data;

public sealed class FixFlowContext(DbContextOptions<FixFlowContext> options) : DbContext(options)
{
    public DbSet<Repair> Repairs => Set<Repair>();
}
