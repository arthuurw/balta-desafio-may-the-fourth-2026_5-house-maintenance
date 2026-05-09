'use client'

import { useState } from 'react'
import type { Repair } from '@/types'
import { createRepair, ApiError } from '@/services/api'

interface Props {
  sessionId: string
  onCreated: (repair: Repair) => void
}

export function RepairForm({ sessionId, onCreated }: Props) {
  const [description, setDescription] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!description.trim()) return
    setLoading(true)
    setError(null)
    try {
      const repair = await createRepair(sessionId, description.trim())
      onCreated(repair)
      setDescription('')
    } catch (err) {
      setError(err instanceof ApiError ? `Erro ${err.status}: ${err.message}` : 'Erro ao adicionar reparo.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-2">
      <div className="flex gap-2">
        <input
          type="text"
          value={description}
          onChange={e => setDescription(e.target.value)}
          placeholder="Ex: Trocar lâmpada do corredor"
          maxLength={500}
          disabled={loading}
          className="flex-1 rounded-lg border border-zinc-700 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-zinc-500 focus:outline-none focus:ring-1 focus:ring-zinc-500 disabled:opacity-50"
        />
        <button
          type="submit"
          disabled={loading || !description.trim()}
          className="rounded-lg bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-zinc-200 disabled:opacity-40 transition-colors"
        >
          {loading ? 'Adicionando…' : 'Adicionar'}
        </button>
      </div>
      {error && <p className="text-xs text-red-400">{error}</p>}
    </form>
  )
}
