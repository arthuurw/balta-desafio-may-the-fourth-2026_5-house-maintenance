using FixFlow.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

var agentFile = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "agents", "agente-reparos.md"));
if (!File.Exists(agentFile))
    throw new InvalidOperationException($"Agent instructions file not found: {agentFile}");

builder.Services.AddFixFlow(builder.Configuration, agentFile);

var app = builder.Build();

await app.Services.EnsureDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapFixFlowEndpoints();

await app.RunAsync();
