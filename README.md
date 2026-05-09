<img width="1280" height="630" alt="banner" src="https://github.com/user-attachments/assets/eb2f345f-7b28-41d0-b374-6336dc8f8f75" />

## 🚀 May The Fourth 2026 — Desafio 5

**FixFlow** — organizador de reparos domésticos com IA. Você lista pequenos reparos (trocar lâmpada, fixar quadro, limpar ralo) e o agente organiza a ordem de execução agrupando tarefas pelo mesmo kit de ferramentas, minimizando trocas e bagunça.

Desenvolvido por **Arthur Webster** — [balta.io](https://balta.io) · May The Fourth 2026.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 Minimal API — Vertical Slice |
| Frontend | Next.js 16 + TypeScript + Tailwind CSS v4 |
| Agente | Microsoft Agent Framework (MAF) 1.4.0 |
| LLM | Groq — `llama-3.3-70b-versatile` |
| Banco | SQLite via EF Core 10 |
| Testes BE | xUnit + NSubstitute + WebApplicationFactory (45 testes) |
| Testes FE | Vitest 2 + Testing Library (23 testes) |

---

## Estrutura

```
backend/
  FixFlow.slnx               ← solution file
  agents/
    agente-reparos.md        ← instruções do agente MAF (15 ações)
  FixFlow.Api/               ← Minimal API, Vertical Slice
  FixFlow.Api.Tests/         ← xUnit + NSubstitute
frontend/                    ← Next.js 16
```

---

## Como rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- Chave de API Groq — [console.groq.com/keys](https://console.groq.com/keys)

### Backend

```bash
# 1. Criar appsettings.Development.json em backend/FixFlow.Api/
{
  "Llm": {
    "ApiKey": "gsk_SUA_CHAVE",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "Model": "llama-3.3-70b-versatile"
  },
  "ConnectionStrings": {
    "Default": "Data Source=fixflow.db"
  }
}

# 2. Rodar
cd backend/FixFlow.Api
dotnet run --urls http://localhost:5059
```

Swagger disponível em `http://localhost:5059/swagger`.

### Frontend

```bash
# 1. Criar frontend/.env.local
NEXT_PUBLIC_API_URL=http://localhost:5059

# 2. Instalar e rodar
cd frontend
npm install
npm run dev
```

App disponível em `http://localhost:3000`.

---

## Testes

```bash
# Backend (40 testes)
cd backend
dotnet test FixFlow.slnx

# Frontend (18 testes)
cd frontend
npm test
```

---

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | /api/repairs | Criar reparo |
| GET | /api/repairs?sessionId=X | Listar por sessão |
| PATCH | /api/repairs/{id} | Editar descrição e/ou categoria |
| PATCH | /api/repairs/{id}/done | Marcar como concluído |
| PATCH | /api/repairs/{id}/reopen | Reabrir reparo concluído |
| DELETE | /api/repairs/{id} | Remover reparo |
| POST | /api/plan | Gerar plano de execução via IA |
| POST | /api/chat | Chat com agente |

---

## Badge

<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/may-the-fourth-2026/rewards/house-maintenance/image.png" width="200" />

---

## Referências

- [MAF Overview](https://learn.microsoft.com/pt-br/agent-framework/overview/?pivots=programming-language-csharp)
- [Groq API Keys](https://console.groq.com/keys)
- [Imersão MAF — YouTube](https://www.youtube.com/watch?v=XkgjeBurtFw)
- [Curso MAF — balta.io](https://balta.io/cursos/fundamentos-do-microsoft-agent-framework)
