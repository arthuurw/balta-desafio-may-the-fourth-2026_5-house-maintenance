using FixFlow.Api.Agents;
using FixFlow.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FixFlow.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IRepairAgent Agent { get; } = Substitute.For<IRepairAgent>();

    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"fixflow-test-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDesc = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<FixFlowContext>));
            if (dbDesc is not null) services.Remove(dbDesc);
            services.AddDbContext<FixFlowContext>(opt =>
                opt.UseSqlite($"Data Source={_dbPath}"));

            var agentDesc = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IRepairAgent));
            if (agentDesc is not null) services.Remove(agentDesc);
            services.AddSingleton(Agent);
        });
    }

    public async Task InitializeAsync()
    {
        _ = CreateClient(); // trigger app startup + EnsureCreated
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
