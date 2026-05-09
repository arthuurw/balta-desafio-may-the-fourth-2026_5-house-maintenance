'use client'

import { useState, useEffect, useCallback } from 'react'
import { RepairForm } from '@/components/RepairForm'
import { RepairList } from '@/components/RepairList'
import { PlanView } from '@/components/PlanView'
import { ChatAgent } from '@/components/ChatAgent'
import { HelpGuide } from '@/components/HelpGuide'
import { listRepairs } from '@/services/api'
import type { Repair } from '@/types'

const TABS = ['Reparos', 'Plano', 'Chat', 'Ajuda'] as const
type Tab = typeof TABS[number]

function getOrCreateSessionId(): string {
  if (typeof window === 'undefined') return ''
  let id = localStorage.getItem('fixflow_session')
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem('fixflow_session', id)
  }
  return id
}

export default function Home() {
  const [tab, setTab] = useState<Tab>('Reparos')
  const [sessionId] = useState(getOrCreateSessionId)
  const [repairs, setRepairs] = useState<Repair[]>([])
  const [loadingRepairs, setLoadingRepairs] = useState(true)

  const fetchRepairs = useCallback(async () => {
    if (!sessionId) return
    try {
      const data = await listRepairs(sessionId)
      setRepairs(data.repairs)
    } catch {
      // silent — user sees empty state
    } finally {
      setLoadingRepairs(false)
    }
  }, [sessionId])

  useEffect(() => {
    fetchRepairs()
  }, [fetchRepairs])

  return (
    <div className="min-h-screen bg-zinc-950">
      <header className="border-b border-zinc-800 bg-zinc-950">
        <div className="mx-auto max-w-2xl px-4 py-4 flex items-baseline gap-3">
          <h1 className="text-lg font-semibold tracking-tight text-zinc-100">FixFlow</h1>
          <span className="text-sm text-zinc-500">Organize seus reparos com IA</span>
        </div>
      </header>

      <div className="mx-auto max-w-2xl px-4">
        <nav className="flex border-b border-zinc-800 mb-6">
          {TABS.map(t => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`px-4 py-3 text-sm font-medium border-b-2 -mb-px transition-colors ${
                tab === t
                  ? 'border-zinc-100 text-zinc-100'
                  : 'border-transparent text-zinc-500 hover:text-zinc-300 hover:border-zinc-600'
              }`}
            >
              {t}
            </button>
          ))}
        </nav>

        {tab === 'Reparos' && (
          <div className="flex flex-col gap-8 pb-10">
            <section>
              <h2 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">
                Novo reparo
              </h2>
              <RepairForm sessionId={sessionId} onCreated={r => setRepairs(prev => [...prev, r])} />
            </section>
            <section>
              <h2 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">
                Meus reparos
              </h2>
              {loadingRepairs
                ? <p className="text-sm text-zinc-500">Carregando…</p>
                : <RepairList repairs={repairs} onUpdate={fetchRepairs} />}
            </section>
          </div>
        )}

        {tab === 'Plano' && (
          <div className="pb-10">
            <PlanView sessionId={sessionId} repairs={repairs} />
          </div>
        )}

        {tab === 'Chat' && (
          <div className="pb-10">
            <ChatAgent sessionId={sessionId} onRepairsChange={fetchRepairs} />
          </div>
        )}

        {tab === 'Ajuda' && (
          <div className="pb-10">
            <HelpGuide />
          </div>
        )}
      </div>
    </div>
  )
}
