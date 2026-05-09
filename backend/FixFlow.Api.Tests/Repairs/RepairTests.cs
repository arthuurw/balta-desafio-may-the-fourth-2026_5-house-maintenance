using System.Net;
using System.Net.Http.Json;
using FixFlow.Api.Features.Repairs;
using Xunit;

namespace FixFlow.Api.Tests.Repairs;

public sealed class RepairTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateRepair_ValidRequest_Returns201()
    {
        var res = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = NewSession(), description = "Trocar lâmpada" });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<RepairDto>();
        Assert.NotNull(body);
        Assert.Equal("pendente", body!.Status);
        Assert.Equal("Trocar lâmpada", body.Description);
    }

    [Fact]
    public async Task CreateRepair_EmptySessionId_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = "", description = "Trocar lâmpada" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateRepair_SessionIdTooLong_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = new string('x', 129), description = "Trocar lâmpada" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateRepair_EmptyDescription_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = NewSession(), description = "" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateRepair_DescriptionTooLong_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = NewSession(), description = new string('a', 501) });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task ListRepairs_ReturnsOnlySessionRepairs()
    {
        var session = NewSession();
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Reparo 1" });
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Reparo 2" });
        await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = NewSession(), description = "Outro" });

        var res = await _client.GetAsync($"/api/repairs?sessionId={session}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<ListRepairs.Response>();
        Assert.Equal(2, body!.Repairs.Count);
    }

    [Fact]
    public async Task ListRepairs_EmptySessionId_Returns400()
    {
        var res = await _client.GetAsync("/api/repairs?sessionId=");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task DoneRepair_ExistingRepair_Returns200WithConcluido()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Fixar quadro" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.PatchAsync($"/api/repairs/{repair!.Id}/done", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<RepairDto>();
        Assert.Equal("concluido", body!.Status);
    }

    [Fact]
    public async Task DoneRepair_NonExistentId_Returns404()
    {
        var res = await _client.PatchAsync($"/api/repairs/{Guid.NewGuid()}/done", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task DeleteRepair_ExistingRepair_Returns204()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Limpar ralo" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.DeleteAsync($"/api/repairs/{repair!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

        var list = await _client.GetAsync($"/api/repairs?sessionId={session}");
        var body = await list.Content.ReadFromJsonAsync<ListRepairs.Response>();
        Assert.Empty(body!.Repairs);
    }

    [Fact]
    public async Task DeleteRepair_NonExistentId_Returns404()
    {
        var res = await _client.DeleteAsync($"/api/repairs/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task EditRepair_UpdatesDescription_Returns200()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar lâmpada velha" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.PatchAsJsonAsync($"/api/repairs/{repair!.Id}",
            new { description = "Trocar lâmpada do corredor" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<RepairDto>();
        Assert.Equal("Trocar lâmpada do corredor", body!.Description);
    }

    [Fact]
    public async Task EditRepair_UpdatesCategory_Returns200()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar torneira" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.PatchAsJsonAsync($"/api/repairs/{repair!.Id}",
            new { category = "hidraulico" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<RepairDto>();
        Assert.Equal("hidraulico", body!.Category);
    }

    [Fact]
    public async Task EditRepair_NoFields_Returns400()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar lâmpada" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.PatchAsJsonAsync($"/api/repairs/{repair!.Id}",
            new { });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task EditRepair_InvalidCategory_Returns400()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Trocar lâmpada" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        var res = await _client.PatchAsJsonAsync($"/api/repairs/{repair!.Id}",
            new { category = "invalido" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task EditRepair_NonExistentId_Returns404()
    {
        var res = await _client.PatchAsJsonAsync($"/api/repairs/{Guid.NewGuid()}",
            new { description = "Novo texto" });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task ReopenRepair_ConcludedRepair_ReturnsPendente()
    {
        var session = NewSession();
        var create = await _client.PostAsJsonAsync("/api/repairs",
            new { sessionId = session, description = "Fixar quadro" });
        var repair = await create.Content.ReadFromJsonAsync<RepairDto>();

        await _client.PatchAsync($"/api/repairs/{repair!.Id}/done", null);

        var res = await _client.PatchAsync($"/api/repairs/{repair.Id}/reopen", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<RepairDto>();
        Assert.Equal("pendente", body!.Status);
    }

    [Fact]
    public async Task ReopenRepair_NonExistentId_Returns404()
    {
        var res = await _client.PatchAsync($"/api/repairs/{Guid.NewGuid()}/reopen", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    private static string NewSession() => Guid.NewGuid().ToString();
}
