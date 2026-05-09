'use client'

import { useEffect, useRef, useState } from 'react'
import type { PlanResponse, Repair } from '@/types'
import { generatePlan, ApiError } from '@/services/api'

interface Props {
  sessionId: string
  repairs: Repair[]
}

const MAX_ATTEMPTS = 3
const COOLDOWN_SECONDS = 15

export function PlanView({ sessionId, repairs }: Props) {
  const [plan, setPlan] = useState<PlanResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [attempts, setAttempts] = useState(0)
  const [cooldownLeft, setCooldownLeft] = useState(0)
  const cooldownRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const repairMap = Object.fromEntries(repairs.map(r => [r.id, r]))
  const pendingCount = repairs.filter(r => r.status === 'pendente').length
  const exhausted = attempts >= MAX_ATTEMPTS
  const onCooldown = cooldownLeft > 0

  useEffect(() => () => { if (cooldownRef.current) clearInterval(cooldownRef.current) }, [])

  function startCooldown() {
    setCooldownLeft(COOLDOWN_SECONDS)
    cooldownRef.current = setInterval(() => {
      setCooldownLeft(prev => {
        if (prev <= 1) {
          clearInterval(cooldownRef.current!)
          return 0
        }
        return prev - 1
      })
    }, 1000)
  }

  async function handleGeneratePlan() {
    if (loading || onCooldown || exhausted) return
    setLoading(true)
    setError(null)
    try {
      const result = await generatePlan(sessionId)
      setPlan(result)
      setAttempts(prev => prev + 1)
      if (result.groups.length === 0) {
        setError('O agente não retornou grupos. Verifique se os reparos têm descrições claras e tente novamente.')
        startCooldown()
      }
    } catch (err) {
      setAttempts(prev => prev + 1)
      if (err instanceof ApiError) {
        if (err.status === 404) setError('Nenhum reparo pendente para planejar.')
        else if (err.status === 502) setError('Erro na comunicação com a IA. Tente novamente.')
        else setError(`Erro ${err.status}: ${err.message}`)
      } else {
        setError('Erro inesperado.')
      }
      startCooldown()
    } finally {
      setLoading(false)
    }
  }

  const attemptsLeft = MAX_ATTEMPTS - attempts
  const buttonLabel = loading
    ? 'Gerando…'
    : onCooldown
      ? `Aguarde ${cooldownLeft}s…`
      : exhausted
        ? 'Limite atingido'
        : 'Gerar plano'

  return (
    <div className="flex flex-col gap-5">
      <div className="flex items-center justify-between">
        <p className="text-sm text-zinc-500">
          {pendingCount === 0
            ? 'Nenhum reparo pendente.'
            : `${pendingCount} reparo${pendingCount !== 1 ? 's' : ''} pendente${pendingCount !== 1 ? 's' : ''}.`}
        </p>
        <div className="flex items-center gap-2">
          {attempts > 0 && !exhausted && (
            <span className="text-xs text-zinc-600">
              {attemptsLeft} tentativa{attemptsLeft !== 1 ? 's' : ''} restante{attemptsLeft !== 1 ? 's' : ''}
            </span>
          )}
          <button
            onClick={handleGeneratePlan}
            disabled={loading || pendingCount === 0 || onCooldown || exhausted}
            className="rounded-lg bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-zinc-200 disabled:opacity-40 transition-colors"
          >
            {buttonLabel}
          </button>
        </div>
      </div>

      {exhausted && (
        <p className="rounded-lg border border-amber-800 bg-amber-950 px-3 py-2 text-sm text-amber-400">
          Limite de {MAX_ATTEMPTS} tentativas atingido. Recarregue a página para tentar novamente.
        </p>
      )}

      {error && !exhausted && (
        <p className="rounded-lg border border-red-900 bg-red-950 px-3 py-2 text-sm text-red-400">{error}</p>
      )}

      {plan && plan.groups.length > 0 && (
        <div className="flex flex-col gap-3">
          <div className="rounded-lg border border-zinc-800 bg-zinc-900 px-4 py-3">
            <p className="text-sm text-zinc-300">{plan.reply}</p>
          </div>

          {plan.groups.map(group => (
            <div key={group.group} className="rounded-lg border border-zinc-800 bg-zinc-900 overflow-hidden">
              <div className="flex items-center gap-3 px-4 py-3 border-b border-zinc-800 bg-zinc-800/50">
                <span className="flex h-6 w-6 items-center justify-center rounded-full bg-zinc-100 text-xs font-semibold text-zinc-900">
                  {group.group}
                </span>
                <h3 className="text-sm font-semibold text-zinc-200">{group.kitName}</h3>
              </div>
              <div className="px-4 py-3">
                <div className="mb-3 flex flex-wrap gap-1.5">
                  {group.tools.map(t => (
                    <span key={t} className="rounded-md border border-zinc-700 bg-zinc-800 px-2 py-0.5 text-xs text-zinc-400">
                      {t}
                    </span>
                  ))}
                </div>
                <ol className="flex flex-col gap-1">
                  {group.repairIds.map((rid, i) => {
                    const r = repairMap[rid]
                    return (
                      <li key={rid} className="flex items-baseline gap-2 text-sm text-zinc-300">
                        <span className="text-zinc-600 tabular-nums text-xs w-4 shrink-0">{i + 1}.</span>
                        <span>{r ? r.description : rid}</span>
                      </li>
                    )
                  })}
                </ol>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
