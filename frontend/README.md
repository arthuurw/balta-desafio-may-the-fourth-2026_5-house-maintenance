# FixFlow — Frontend

Next.js 16 + TypeScript + Tailwind CSS v4. Interface do organizador de reparos domésticos com agente de IA.

---

## Stack

| | |
|---|---|
| Framework | Next.js 16 (App Router) |
| Linguagem | TypeScript |
| Estilos | Tailwind CSS v4 |
| Testes | Vitest 2 + Testing Library |

---

## Estrutura

```
src/
  app/
    layout.tsx        ← root layout (Geist fonts)
    page.tsx          ← SPA com 4 tabs
    globals.css       ← Tailwind v4 (@import "tailwindcss")
  components/
    RepairForm.tsx    ← formulário de criação de reparo
    RepairList.tsx    ← lista pendentes / concluídos
    PlanView.tsx      ← visualização do plano por kit
    ChatAgent.tsx     ← interface de chat com o agente
    HelpGuide.tsx     ← guia de comandos
  services/
    api.ts            ← funções de acesso à API
  types/
    index.ts          ← interfaces TypeScript
  test/
    api.test.ts       ← 23 testes das funções de api.ts
    setup.ts
```

---

## Como rodar

### Pré-requisitos

- Node.js 20+
- Backend rodando em `http://localhost:5059`

### Desenvolvimento

```bash
# 1. Criar .env.local
echo "NEXT_PUBLIC_API_URL=http://localhost:5059" > .env.local

# 2. Instalar dependências
npm install

# 3. Rodar
npm run dev
```

App disponível em `http://localhost:3000`.

### Testes

```bash
npm test
```

### Build

```bash
npm run build
npm start
```

---

## Variáveis de ambiente

| Variável | Padrão | Descrição |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | `http://localhost:5059` | URL base do backend |

---

## Funções de API (`services/api.ts`)

| Função | Método | Rota |
|---|---|---|
| `createRepair(sessionId, description)` | POST | `/api/repairs` |
| `listRepairs(sessionId)` | GET | `/api/repairs?sessionId=X` |
| `editRepair(id, fields)` | PATCH | `/api/repairs/{id}` |
| `markDone(id)` | PATCH | `/api/repairs/{id}/done` |
| `reopenRepair(id)` | PATCH | `/api/repairs/{id}/reopen` |
| `deleteRepair(id)` | DELETE | `/api/repairs/{id}` |
| `generatePlan(sessionId)` | POST | `/api/plan` |
| `chat(sessionId, message)` | POST | `/api/chat` |

Todas lançam `ApiError(status, message)` em resposta HTTP de erro. Timeout de 30s por request.

---

## Ações do agente (`ChatResponse.action`)

| Action | Efeito no backend | Campos extras na resposta |
|---|---|---|
| `add_repair` | persiste 1 reparo | `createdRepairs[]` |
| `bulk_add` | persiste N reparos | `createdRepairs[]` |
| `edit_repair` | atualiza reparo | `editedRepair` |
| `mark_done` | conclui reparo | `markedDoneId` |
| `undo_done` | reabre reparo | `reopenedId` |
| `remove_repair` | remove reparo | `removedId` |
| `generate_plan` | — | `planGroups[]` |
| `list_repairs` | — | `repairs[]` |
| `suggest_next` | — | `nextRepair` |
| `shopping_list` | — | `shoppingList[]` |
| `estimate_time` | — | `timeEstimates[]` |
| `session_summary` | — | — |
| `ask_clarification` | — | — |
| `general_reply` | — | — |
| `unknown` | — | — |

---

## Notas

- `sessionId` gerado no `localStorage` na primeira visita, reutilizado em todas as requests
- Sem SSR para dados — tudo client-side com `useEffect`
- Tailwind v4: usa `@import "tailwindcss"` — **não** usar `@tailwind base/components/utilities`
