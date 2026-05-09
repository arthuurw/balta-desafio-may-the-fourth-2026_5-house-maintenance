using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FixFlow.Api.Agents;

internal sealed class RepairAgent(ChatClientAgent agent, ILogger<RepairAgent> logger) : IRepairAgent
{
    public async Task<AgentResponseDto> AskAsync(string prompt, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var session = await agent.CreateSessionAsync(ct);
            var runOptions = new ChatClientAgentRunOptions(new ChatOptions());

            try
            {
                var response = await agent.RunAsync(prompt, session, runOptions, ct);

                logger.LogInformation(
                    "Agent usage — attempt {Attempt}: input={Input} output={Output} total={Total}",
                    attempt + 1,
                    response.Usage?.InputTokenCount,
                    response.Usage?.OutputTokenCount,
                    response.Usage?.TotalTokenCount);

                if (AgentResponseDto.TryParse(response.Text, out var dto))
                    return dto!;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new AgentException($"Agent call failed: {ex.Message}", ex);
            }
        }

        throw new AgentException("LLM returned invalid JSON after 2 attempts.");
    }
}
