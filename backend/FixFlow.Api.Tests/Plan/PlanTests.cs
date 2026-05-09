using System.Net;
using System.Net.Http.Json;
using FixFlow.Api.Agents;
using FixFlow.Api.Features.Plan;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace FixFlow.Api.Tests.Plan;

public sealed class PlanTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly IRepairAgent _agent = factory.Agent;

    [Fact]
    public async Task GeneratePlan_WithPendingRepairs_Returns200()
    {
        var session = NewSession();
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar lâmpada" });

        _agent.AskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AgentResponseDto
            {
                Action = "generate_plan",
                Reply = "Plano gerado!",
                PlanGroups = [new PlanGroupDto(1, "Kit Elétrico", ["escada"], [])]
            });

        var res = await _client.PostAsJsonAsync("/api/plan", new { sessionId = session });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<GeneratePlan.Response>();
        Assert.Equal("Plano gerado!", body!.Reply);
        Assert.Single(body.Groups);
        Assert.Equal("Kit Elétrico", body.Groups[0].KitName);
    }

    [Fact]
    public async Task GeneratePlan_EmptySessionId_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/plan", new { sessionId = "" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GeneratePlan_NoPendingRepairs_Returns404()
    {
        var res = await _client.PostAsJsonAsync("/api/plan", new { sessionId = NewSession() });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GeneratePlan_AgentException_Returns502()
    {
        var session = NewSession();
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Fixar quadro" });

        _agent.AskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AgentException("LLM error"));

        var res = await _client.PostAsJsonAsync("/api/plan", new { sessionId = session });
        Assert.Equal(HttpStatusCode.BadGateway, res.StatusCode);
    }

    [Fact]
    public async Task GeneratePlan_MultipleRepairs_ReturnsPlanGroups()
    {
        var session = NewSession();
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar lâmpada" });
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Fixar quadro" });
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Limpar ralo" });

        _agent.AskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AgentResponseDto
            {
                Action = "generate_plan",
                Reply = "3 reparos em 3 grupos.",
                PlanGroups =
                [
                    new PlanGroupDto(1, "Kit Elétrico", ["escada", "lâmpada"], []),
                    new PlanGroupDto(2, "Kit de Fixação", ["martelo", "prego"], []),
                    new PlanGroupDto(3, "Kit de Limpeza", ["luvas", "desentupidor"], []),
                ]
            });

        var res = await _client.PostAsJsonAsync("/api/plan", new { sessionId = session });
        var body = await res.Content.ReadFromJsonAsync<GeneratePlan.Response>();
        Assert.Equal(3, body!.Groups.Length);
    }

    private static string NewSession() => Guid.NewGuid().ToString();
}
