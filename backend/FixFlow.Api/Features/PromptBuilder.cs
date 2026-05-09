using System.Text;
using FixFlow.Api.Data.Entities;

namespace FixFlow.Api.Features;

static class PromptBuilder
{
    private static readonly TimeZoneInfo Brt =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "E. South America Standard Time"
                : "America/Sao_Paulo");

    private static readonly string[] DayNames =
        ["domingo", "segunda", "terça", "quarta", "quinta", "sexta", "sábado"];

    // Prevents prompt injection: strips newlines and escapes bracket delimiters
    // used as section markers in the prompt template.
    private static string S(string? value)
        => (value ?? string.Empty)
            .ReplaceLineEndings(" ")
            .Replace("[", "⟦")
            .Replace("]", "⟧");

    public static string Build(string sessionId, IEnumerable<Repair> repairs, string userMessage)
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brt);
        var day = DayNames[(int)now.DayOfWeek];

        var pending = repairs.Where(r => r.Status == "pendente").ToList();
        var done = repairs.Where(r => r.Status == "concluido").ToList();

        var safeSessionId = S(sessionId);

        var sb = new StringBuilder();
        sb.AppendLine($"[Data atual: {day}, {now:dd/MM/yyyy}]");
        sb.AppendLine();

        if (pending.Count > 0)
        {
            sb.AppendLine($"[Reparos pendentes — Sessão: {safeSessionId}]");
            foreach (var r in pending)
            {
                var tools = r.Tools();
                var toolsStr = tools.Length > 0 ? string.Join(", ", tools.Select(S)) : "não informado";
                sb.AppendLine($"id:{r.Id} — \"{S(r.Description)}\" — {S(r.Category ?? "geral")} — ferramentas: [{toolsStr}]");
            }
            sb.AppendLine();
        }

        if (done.Count > 0)
        {
            sb.AppendLine($"[Reparos concluídos — Sessão: {safeSessionId}]");
            foreach (var r in done)
                sb.AppendLine($"id:{r.Id} — \"{S(r.Description)}\" — concluido");
            sb.AppendLine();
        }

        sb.AppendLine("[Mensagem do usuário]");
        sb.Append(userMessage);

        return sb.ToString();
    }
}
