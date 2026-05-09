using FixFlow.Api.Agents;
using FixFlow.Api.Data;
using FixFlow.Api.Features.Repairs;
using FixFlow.Api.Infrastructure.Llm;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Api.Extensions;

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFixFlow(this IServiceCollection services, IConfiguration config, string agentFilePath)
    {
        services.AddDbContext<FixFlowContext>(opt =>
            opt.UseSqlite(config.GetConnectionString("Default")));

        services.AddSingleton<Microsoft.Agents.AI.ChatClientAgent>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return GroqClientFactory.Create(config, agentFilePath, loggerFactory, sp);
        });

        services.AddSingleton<IRepairAgent, RepairAgent>();

        services.AddValidatorsFromAssemblyContaining<CreateRepair.Validator>(includeInternalTypes: true);

        services.AddCors(opt =>
            opt.AddDefaultPolicy(p =>
                p.WithOrigins("http://localhost:3000", "http://localhost:3001")
                 .AllowAnyHeader()
                 .AllowAnyMethod()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
            opt.CustomSchemaIds(t => t.FullName?.Replace("+", "_")));

        return services;
    }

    public static async Task EnsureDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FixFlowContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
