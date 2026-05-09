using FixFlow.Api.Features.Chat;
using FixFlow.Api.Features.Plan;
using FixFlow.Api.Features.Repairs;

namespace FixFlow.Api.Extensions;

static class WebApplicationExtensions
{
    public static WebApplication MapFixFlowEndpoints(this WebApplication app)
    {
        var repairs = app.MapGroup("/api/repairs").WithTags("Repairs");
        repairs.MapPost("/", CreateRepair.HandleAsync).WithName("CreateRepair");
        repairs.MapGet("/", ListRepairs.HandleAsync).WithName("ListRepairs");
        repairs.MapPatch("/{id:guid}/done", DoneRepair.HandleAsync).WithName("DoneRepair");
        repairs.MapPatch("/{id:guid}/reopen", ReopenRepair.HandleAsync).WithName("ReopenRepair");
        repairs.MapPatch("/{id:guid}", EditRepair.HandleAsync).WithName("EditRepair");
        repairs.MapDelete("/{id:guid}", DeleteRepair.HandleAsync).WithName("DeleteRepair");

        app.MapPost("/api/plan", GeneratePlan.HandleAsync)
           .WithTags("Plan")
           .WithName("GeneratePlan");

        app.MapPost("/api/chat", Chat.HandleAsync)
           .WithTags("Chat")
           .WithName("Chat");

        return app;
    }
}
