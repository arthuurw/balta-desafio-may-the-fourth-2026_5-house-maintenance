'use client'

import { useState } from 'react'
import type { Repair } from '@/types'
import { markDone, deleteRepair, ApiError } from '@/services/api'

interface Props {
  repairs: Repair[]
  onUpdate: () => void
}

const categoryLabels: Record<string, string> = {
  eletrico: 'Elétrico',
  hidraulico: 'Hidráulico',
  fixacao: 'Fixação',
  limpeza: 'Limpeza',
  geral: 'Geral',
}

export function RepairList({ repairs, onUpdate }: Props) {
  const [loadingId, setLoadingId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  async function handleDone(id: string) {
    setLoadingId(id)
    setError(null)
    try {
      await markDone(id)
      onUpdate()
    } catch (err) {
      setError(err instanceof ApiError ? `Erro ${err.status}` : 'Erro ao concluir.')
    } finally {
      setLoadingId(null)
    }
  }

  async function handleDelete(id: string) {
    setLoadingId(id)
    setError(null)
    try {
      await deleteRepair(id)
      onUpdate()
    } catch (err) {
      setError(err instanceof ApiError ? `Erro ${err.status}` : 'Erro ao remover.')
    } finally {
      setLoadingId(null)
    }
  }

  if (repairs.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-zinc-700 p-8 text-center">
        <p className="text-sm text-zinc-500">Nenhum reparo cadastrado.</p>
      </div>
    )
  }

  const pending = repairs.filter(r => r.status === 'pendente')
  const done = repairs.filter(r => r.status === 'concluido')

  return (
    <div className="flex flex-col gap-6">
      {error && <p className="text-xs text-red-400">{error}</p>}

      {pending.length > 0 && (
        <div>
          <p className="mb-2 text-xs font-semibold text-zinc-500 uppercase tracking-widest">
            Pendentes ({pending.length})
          </p>
          <ul className="flex flex-col gap-1.5">
            {pending.map(r => (
              <li key={r.id} className="flex items-start gap-3 rounded-lg border border-zinc-800 bg-zinc-900 px-3 py-2.5">
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-zinc-200">{r.description}</p>
                  {(r.category || r.tools.length > 0) && (
                    <div className="mt-1.5 flex flex-wrap gap-1">
                      {r.category && (
                        <span className="rounded-md bg-zinc-800 border border-zinc-700 px-2 py-0.5 text-xs text-zinc-400">
                          {categoryLabels[r.category] ?? r.category}
                        </span>
                      )}
                      {r.tools.map(t => (
                        <span key={t} className="rounded-md bg-zinc-800 border border-zinc-700 px-2 py-0.5 text-xs text-zinc-500">
                          {t}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
                <div className="flex gap-0.5 shrink-0 mt-0.5">
                  <button
                    onClick={() => handleDone(r.id)}
                    disabled={loadingId === r.id}
                    title="Marcar como concluído"
                    className="rounded p-1.5 text-zinc-600 hover:text-emerald-400 hover:bg-emerald-950 disabled:opacity-40 transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                  </button>
                  <button
                    onClick={() => handleDelete(r.id)}
                    disabled={loadingId === r.id}
                    title="Remover reparo"
                    className="rounded p-1.5 text-zinc-600 hover:text-red-400 hover:bg-red-950 disabled:opacity-40 transition-colors"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}

      {done.length > 0 && (
        <div>
          <p className="mb-2 text-xs font-semibold text-zinc-600 uppercase tracking-widest">
            Concluídos ({done.length})
          </p>
          <ul className="flex flex-col gap-1.5">
            {done.map(r => (
              <li key={r.id} className="flex items-center gap-3 rounded-lg border border-zinc-800 bg-zinc-900/50 px-3 py-2.5 opacity-60">
                <p className="flex-1 text-sm text-zinc-500 line-through">{r.description}</p>
                <button
                  onClick={() => handleDelete(r.id)}
                  disabled={loadingId === r.id}
                  title="Remover"
                  className="rounded p-1.5 text-zinc-700 hover:text-zinc-400 hover:bg-zinc-800 disabled:opacity-40 transition-colors shrink-0"
                >
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
