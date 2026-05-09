# FixFlow — Design Notes

May The Fourth 2026 — Desafio 5

## Visual Identity

Clean, functional aesthetic. Dark background with high-contrast text. Tool badges use soft colors to distinguish categories.

## Color Palette

| Token | Hex | Use |
|---|---|---|
| `--bg` | `#0f1117` | Page background |
| `--surface` | `#1a1d27` | Card / panel background |
| `--border` | `#2d3047` | Borders, dividers |
| `--primary` | `#6366f1` | Primary actions (buttons, links) |
| `--success` | `#22c55e` | Completed repairs, success states |
| `--danger` | `#ef4444` | Delete actions, errors |
| `--text` | `#e2e8f0` | Primary text |
| `--muted` | `#94a3b8` | Secondary text, placeholders |

## Typography

Inter (system-ui fallback). No special font loading.

- Headings: `font-semibold`, size 18–24px
- Body: 14–16px
- Labels/badges: 12px, `font-medium`

## Layout

Single-page app with 4 tabs: **Reparos / Plano / Chat / Ajuda**.

- Max content width: 800px, centered
- Tab bar: sticky, top of page
- Cards: 8px radius, 1px border
- Spacing unit: 4px (Tailwind default scale)

## Components

### Repair Card
- Description text (primary)
- Status badge: `pendente` (yellow) | `concluido` (green)
- Tool pills: one pill per tool, muted blue
- Actions: "Concluído" button + trash icon

### Plan Group
- Numbered header (Group 1, 2, 3…)
- Kit name bold
- Tool badges row
- Nested repair list with descriptions

### Chat Bubble
- User: right-aligned, primary background
- Agent: left-aligned, surface background
- Action-specific renders: plan groups inline, repair cards for `add_repair`/`bulk_add`, shopping list for `shopping_list`, time estimates for `estimate_time`

## Tailwind CSS v4

Uses `@import "tailwindcss"` (not v3 directives). Custom tokens via `@theme {}` block in `globals.css`.

## Responsive

Mobile-first. Single column. No sidebar. Tab bar collapses to icons below 400px.
