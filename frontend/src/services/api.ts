const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5059'

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const controller = new AbortController()
  const timer = setTimeout(() => controller.abort(), 30_000)
  try {
    const res = await fetch(`${BASE_URL}${path}`, {
      ...init,
      signal: controller.signal,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    })
    if (!res.ok) {
      const text = await res.text().catch(() => '')
      throw new ApiError(res.status, text || res.statusText)
    }
    if (res.status === 204) return undefined as unknown as T
    return res.json() as Promise<T>
  } finally {
    clearTimeout(timer)
  }
}

import type { Repair, PlanResponse, ChatResponse } from '@/types'

export function createRepair(sessionId: string, description: string): Promise<Repair> {
  return request('/api/repairs', {
    method: 'POST',
    body: JSON.stringify({ sessionId, description }),
  })
}

export function listRepairs(sessionId: string): Promise<{ repairs: Repair[] }> {
  return request(`/api/repairs?sessionId=${encodeURIComponent(sessionId)}`)
}

export function editRepair(id: string, fields: { description?: string; category?: string }): Promise<Repair> {
  return request(`/api/repairs/${id}`, {
    method: 'PATCH',
    body: JSON.stringify(fields),
  })
}

export function markDone(id: string): Promise<Repair> {
  return request(`/api/repairs/${id}/done`, { method: 'PATCH' })
}

export function reopenRepair(id: string): Promise<Repair> {
  return request(`/api/repairs/${id}/reopen`, { method: 'PATCH' })
}

export function deleteRepair(id: string): Promise<void> {
  return request(`/api/repairs/${id}`, { method: 'DELETE' })
}

export function generatePlan(sessionId: string): Promise<PlanResponse> {
  return request('/api/plan', {
    method: 'POST',
    body: JSON.stringify({ sessionId }),
  })
}

export function chat(sessionId: string, message: string): Promise<ChatResponse> {
  return request('/api/chat', {
    method: 'POST',
    body: JSON.stringify({ sessionId, message }),
  })
}
