namespace FixFlow.Api.Agents;

public interface IRepairAgent
{
    Task<AgentResponseDto> AskAsync(string prompt, CancellationToken ct = default);
}
