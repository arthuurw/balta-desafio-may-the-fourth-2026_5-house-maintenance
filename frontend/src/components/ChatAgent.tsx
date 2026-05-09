'use client'

import { useState, useRef, useEffect } from 'react'
import type { ChatResponse } from '@/types'
import { chat, ApiError } from '@/services/api'

interface Message {
  role: 'user' | 'agent'
  text: string
  data?: ChatResponse
}

interface Props {
  sessionId: string
  onRepairsChange?: () => void
}

const STATE_CHANGING_ACTIONS = ['add_repair', 'bulk_add', 'edit_repair', 'mark_done', 'undo_done', 'remove_repair']

export function ChatAgent({ sessionId, onRepairsChange }: Props) {
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  async function handleSend(e: React.FormEvent) {
    e.preventDefault()
    const text = input.trim()
    if (!text || loading) return
    setInput('')
    setMessages(prev => [...prev, { role: 'user', text }])
    setLoading(true)
    try {
      const result = await chat(sessionId, text)
      setMessages(prev => [...prev, { role: 'agent', text: result.reply, data: result }])
      if (STATE_CHANGING_ACTIONS.includes(result.action)) {
        onRepairsChange?.()
      }
    } catch (err) {
      const msg = err instanceof ApiError
        ? err.status === 502 ? 'Erro na comunicação com a IA. Tente novamente.' : `Erro ${err.status}: ${err.message}`
        : 'Erro inesperado.'
      setMessages(prev => [...prev, { role: 'agent', text: msg }])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex flex-col" style={{ height: 520 }}>
      <div className="flex-1 overflow-y-auto flex flex-col gap-2 py-2 mb-3">
        {messages.length === 0 && (
          <div className="flex items-center justify-center h-full">
            <p className="text-sm text-zinc-600">Como posso ajudar com seus reparos?</p>
          </div>
        )}
        {messages.map((msg, i) => (
          <div key={i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div className={`max-w-[78%] rounded-2xl px-4 py-2.5 ${
              msg.role === 'user'
                ? 'bg-zinc-100 text-zinc-900 rounded-br-sm'
                : 'bg-zinc-900 border border-zinc-800 text-zinc-200 rounded-bl-sm'
            }`}>
              <p className="text-sm leading-relaxed">{msg.text}</p>
              {msg.role === 'agent' && msg.data && <AgentDataView data={msg.data} />}
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="bg-zinc-900 border border-zinc-800 rounded-2xl rounded-bl-sm px-4 py-3">
              <div className="flex gap-1 items-center">
                <span className="w-1.5 h-1.5 rounded-full bg-zinc-500 animate-bounce" style={{ animationDelay: '0ms' }} />
                <span className="w-1.5 h-1.5 rounded-full bg-zinc-500 animate-bounce" style={{ animationDelay: '150ms' }} />
                <span className="w-1.5 h-1.5 rounded-full bg-zinc-500 animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      <form onSubmit={handleSend} className="flex gap-2">
        <input
          type="text"
          value={input}
          onChange={e => setInput(e.target.value)}
          placeholder="Digite uma mensagem…"
          maxLength={1000}
          disabled={loading}
          className="flex-1 rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-2.5 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-zinc-500 focus:outline-none focus:ring-1 focus:ring-zinc-500 disabled:opacity-50"
        />
        <button
          type="submit"
          disabled={loading || !input.trim()}
          className="rounded-xl bg-zinc-100 px-4 py-2.5 text-sm font-medium text-zinc-900 hover:bg-zinc-200 disabled:opacity-40 transition-colors"
        >
          Enviar
        </button>
      </form>
    </div>
  )
}

function AgentDataView({ data }: { data: ChatResponse }) {
  switch (data.action) {
    case 'add_repair':
    case 'bulk_add':
      if (!data.createdRepairs?.length) return null
      return (
        <div className="mt-2 flex flex-col gap-1">
          {data.createdRepairs.map(r => (
            <div key={r.id} className="rounded-lg border border-emerald-900 bg-emerald-950 px-3 py-2 text-xs text-emerald-400">
              <span className="font-medium">Reparo adicionado:</span> {r.description}
              {r.category && <span className="ml-1 text-emerald-600">({r.category})</span>}
            </div>
          ))}
        </div>
      )

    case 'edit_repair':
      if (!data.editedRepair) return null
      return (
        <div className="mt-2 rounded-lg border border-blue-900 bg-blue-950 px-3 py-2 text-xs text-blue-400">
          <span className="font-medium">Reparo atualizado:</span> {data.editedRepair.description}
          {data.editedRepair.category && (
            <span className="ml-1 text-blue-600">({data.editedRepair.category})</span>
          )}
        </div>
      )

    case 'mark_done':
      return (
        <div className="mt-2 inline-block rounded-md border border-emerald-900 bg-emerald-950 px-2.5 py-1 text-xs font-medium text-emerald-400">
          Marcado como concluído
        </div>
      )

    case 'undo_done':
      return (
        <div className="mt-2 inline-block rounded-md border border-amber-900 bg-amber-950 px-2.5 py-1 text-xs font-medium text-amber-400">
          Reparo reaberto
        </div>
      )

    case 'remove_repair':
      return (
        <div className="mt-2 inline-block rounded-md border border-red-900 bg-red-950 px-2.5 py-1 text-xs font-medium text-red-400">
          Reparo removido
        </div>
      )

    case 'list_repairs':
      if (!data.repairs) return null
      return (
        <div className="mt-2 flex flex-col gap-1">
          {data.repairs.length === 0
            ? <p className="text-xs text-zinc-500">Nenhum reparo pendente.</p>
            : data.repairs.map(r => (
              <div key={r.id} className="rounded-md border border-zinc-700 bg-zinc-800 px-2 py-1 text-xs text-zinc-300">
                {r.description}
                {r.category && <span className="ml-1 text-zinc-500">({r.category})</span>}
              </div>
            ))}
        </div>
      )

    case 'suggest_next':
      if (!data.nextRepair) return null
      return (
        <div className="mt-2 rounded-lg border border-amber-900 bg-amber-950 px-3 py-2 text-xs text-amber-400">
          <span className="font-medium">Próximo:</span> {data.nextRepair.description}
          {data.nextRepair.tools && data.nextRepair.tools.length > 0 && (
            <p className="mt-1 text-amber-600">Ferramentas: {data.nextRepair.tools.join(', ')}</p>
          )}
        </div>
      )

    case 'generate_plan':
      if (!data.planGroups) return null
      return (
        <div className="mt-2 flex flex-col gap-1.5">
          {data.planGroups.map(group => (
            <div key={group.group} className="rounded-lg border border-zinc-700 bg-zinc-800 overflow-hidden">
              <div className="flex items-center gap-2 px-2.5 py-1.5 border-b border-zinc-700 bg-zinc-800">
                <span className="flex h-4 w-4 items-center justify-center rounded-full bg-zinc-100 text-zinc-900 text-[10px] font-semibold">
                  {group.group}
                </span>
                <span className="text-xs font-medium text-zinc-300">{group.kitName}</span>
              </div>
              <div className="px-2.5 py-1.5 flex flex-wrap gap-1">
                {group.tools.map(t => (
                  <span key={t} className="rounded-md border border-zinc-700 bg-zinc-900 px-1.5 py-0.5 text-xs text-zinc-500">
                    {t}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      )

    case 'shopping_list':
      if (!data.shoppingList?.length) return null
      return (
        <div className="mt-2 flex flex-col gap-1.5">
          {data.shoppingList.map(kit => (
            <div key={kit.kitName} className="rounded-lg border border-zinc-700 bg-zinc-800 overflow-hidden">
              <div className="px-2.5 py-1.5 border-b border-zinc-700">
                <span className="text-xs font-medium text-zinc-300">{kit.kitName}</span>
              </div>
              <ul className="px-2.5 py-1.5 flex flex-col gap-0.5">
                {kit.items.map(item => (
                  <li key={item} className="text-xs text-zinc-400 before:content-['·'] before:mr-1.5 before:text-zinc-600">
                    {item}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      )

    case 'estimate_time':
      if (!data.timeEstimates?.length) return null
      return (
        <div className="mt-2 flex flex-col gap-1">
          {data.timeEstimates.map(est => (
            <div key={est.repairId} className="flex items-center justify-between rounded-md border border-zinc-700 bg-zinc-800 px-2.5 py-1.5 text-xs">
              <span className="text-zinc-300">{est.description}</span>
              <span className="ml-3 shrink-0 text-zinc-500">{est.estimatedMinutes} min</span>
            </div>
          ))}
          <div className="flex justify-end text-xs text-zinc-500 pt-0.5">
            Total: {data.timeEstimates.reduce((s, e) => s + e.estimatedMinutes, 0)} min
          </div>
        </div>
      )

    default:
      return null
  }
}
