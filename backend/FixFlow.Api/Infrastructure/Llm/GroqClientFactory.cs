using System.ClientModel;
using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using MEAIChatMessage = Microsoft.Extensions.AI.ChatMessage;
using MEAIChatRole = Microsoft.Extensions.AI.ChatRole;
using MEAIChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

namespace FixFlow.Api.Infrastructure.Llm;

internal static class GroqClientFactory
{
    internal static ChatClientAgent Create(
        IConfiguration config,
        string agentFilePath,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        var apiKey = config["Llm:ApiKey"]
            ?? throw new InvalidOperationException("Llm:ApiKey is not configured.");
        var baseUrl = config["Llm:BaseUrl"]
            ?? throw new InvalidOperationException("Llm:BaseUrl is not configured.");
        var model = config["Llm:Model"]
            ?? throw new InvalidOperationException("Llm:Model is not configured.");
        var instructions = File.ReadAllText(agentFilePath);

        var chatClient = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(baseUrl) }
        ).GetChatClient(model);

        return chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = "RepairAgent",
                ChatOptions = new ChatOptions { ResponseFormat = MEAIChatResponseFormat.Json },
            },
            innerClient => innerClient
                .AsBuilder()
                .Use(next => new SystemPromptChatClient(next, instructions))
                .Build(),
            loggerFactory,
            serviceProvider
        );
    }
}

file sealed class SystemPromptChatClient(IChatClient inner, string systemPrompt)
    : DelegatingChatClient(inner)
{
    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<MEAIChatMessage> messages,
        ChatOptions? options,
        CancellationToken cancellationToken = default)
    {
        var msgs = Prepend(messages);
        return base.GetResponseAsync(msgs, options, cancellationToken);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<MEAIChatMessage> messages,
        ChatOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var msgs = Prepend(messages);
        await foreach (var update in base.GetStreamingResponseAsync(msgs, options, cancellationToken))
            yield return update;
    }

    private List<MEAIChatMessage> Prepend(IEnumerable<MEAIChatMessage> messages)
    {
        var msgs = new List<MEAIChatMessage> { new(MEAIChatRole.System, systemPrompt) };
        msgs.AddRange(messages);
        return msgs;
    }
}
