namespace FixFlow.Api.Agents;

public sealed class AgentException : Exception
{
    public AgentException(string message) : base(message) { }
    public AgentException(string message, Exception inner) : base(message, inner) { }
}
