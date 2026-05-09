import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createRepair, listRepairs, editRepair, markDone, reopenRepair, deleteRepair, generatePlan, chat, ApiError } from '@/services/api'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

function mockOk(body: unknown, status = 200) {
  mockFetch.mockResolvedValueOnce({
    ok: true,
    status,
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(''),
  })
}

function mockError(status: number, text = '') {
  mockFetch.mockResolvedValueOnce({
    ok: false,
    status,
    statusText: String(status),
    json: () => Promise.reject(new Error()),
    text: () => Promise.resolve(text),
  })
}

const repair = {
  id: '1',
  sessionId: 'sess',
  description: 'Trocar lâmpada',
  category: 'eletrico',
  tools: ['escada'],
  status: 'pendente' as const,
  createdAt: new Date().toISOString(),
}

beforeEach(() => mockFetch.mockReset())

describe('createRepair', () => {
  it('returns repair on 201', async () => {
    mockOk(repair, 201)
    const r = await createRepair('sess', 'Trocar lâmpada')
    expect(r.description).toBe('Trocar lâmpada')
  })

  it('throws ApiError on 400', async () => {
    mockError(400, 'Description required')
    await expect(createRepair('sess', '')).rejects.toBeInstanceOf(ApiError)
  })
})

describe('listRepairs', () => {
  it('returns repairs array', async () => {
    mockOk({ repairs: [repair] })
    const data = await listRepairs('sess')
    expect(data.repairs).toHaveLength(1)
  })

  it('returns empty array for empty session', async () => {
    mockOk({ repairs: [] })
    const data = await listRepairs('empty-session')
    expect(data.repairs).toHaveLength(0)
  })
})

describe('editRepair', () => {
  it('returns updated repair on 200', async () => {
    mockOk({ ...repair, description: 'Trocar lâmpada do corredor' })
    const r = await editRepair(repair.id, { description: 'Trocar lâmpada do corredor' })
    expect(r.description).toBe('Trocar lâmpada do corredor')
  })

  it('throws ApiError 404 for unknown id', async () => {
    mockError(404)
    const err = await editRepair('unknown', { description: 'X' }).catch(e => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).status).toBe(404)
  })

  it('throws ApiError 400 when no fields provided', async () => {
    mockError(400)
    const err = await editRepair(repair.id, {}).catch(e => e)
    expect((err as ApiError).status).toBe(400)
  })
})

describe('markDone', () => {
  it('returns updated repair', async () => {
    mockOk({ ...repair, status: 'concluido' })
    const r = await markDone(repair.id)
    expect(r.status).toBe('concluido')
  })

  it('throws ApiError 404 for unknown id', async () => {
    mockError(404)
    const err = await markDone('unknown').catch(e => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).status).toBe(404)
  })
})

describe('reopenRepair', () => {
  it('returns repair with pendente status', async () => {
    mockOk({ ...repair, status: 'pendente' })
    const r = await reopenRepair(repair.id)
    expect(r.status).toBe('pendente')
  })

  it('throws ApiError 404 for unknown id', async () => {
    mockError(404)
    const err = await reopenRepair('unknown').catch(e => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).status).toBe(404)
  })
})

describe('deleteRepair', () => {
  it('resolves on 204', async () => {
    mockFetch.mockResolvedValueOnce({ ok: true, status: 204, json: () => Promise.resolve(null), text: () => Promise.resolve('') })
    await expect(deleteRepair(repair.id)).resolves.toBeUndefined()
  })

  it('throws ApiError 404 for unknown id', async () => {
    mockError(404)
    const err = await deleteRepair('unknown').catch(e => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).status).toBe(404)
  })
})

describe('generatePlan', () => {
  it('returns plan with groups', async () => {
    const plan = { reply: 'Plano!', groups: [{ group: 1, kitName: 'Kit Elétrico', tools: ['escada'], repairIds: ['1'] }] }
    mockOk(plan)
    const result = await generatePlan('sess')
    expect(result.groups).toHaveLength(1)
    expect(result.groups[0].kitName).toBe('Kit Elétrico')
  })

  it('throws ApiError 404 when no pending repairs', async () => {
    mockError(404)
    const err = await generatePlan('empty').catch(e => e)
    expect((err as ApiError).status).toBe(404)
  })

  it('throws ApiError 502 on LLM failure', async () => {
    mockError(502)
    const err = await generatePlan('sess').catch(e => e)
    expect((err as ApiError).status).toBe(502)
  })
})

describe('chat', () => {
  it('returns general_reply', async () => {
    mockOk({ action: 'general_reply', reply: 'Olá!' })
    const res = await chat('sess', 'Oi')
    expect(res.action).toBe('general_reply')
    expect(res.reply).toBe('Olá!')
  })

  it('returns add_repair with createdRepairs array', async () => {
    mockOk({ action: 'add_repair', reply: 'Adicionado!', createdRepairs: [repair] })
    const res = await chat('sess', 'Adiciona lâmpada')
    expect(res.action).toBe('add_repair')
    expect(res.createdRepairs?.[0].description).toBe('Trocar lâmpada')
  })

  it('returns mark_done with markedDoneId', async () => {
    mockOk({ action: 'mark_done', reply: 'Concluído!', markedDoneId: '1' })
    const res = await chat('sess', 'Terminei a lâmpada')
    expect(res.action).toBe('mark_done')
    expect(res.markedDoneId).toBe('1')
  })

  it('returns remove_repair with removedId', async () => {
    mockOk({ action: 'remove_repair', reply: 'Removido.', removedId: '1' })
    const res = await chat('sess', 'Remove a lâmpada')
    expect(res.action).toBe('remove_repair')
    expect(res.removedId).toBe('1')
  })

  it('throws ApiError 400 for empty message', async () => {
    mockError(400)
    const err = await chat('sess', '').catch(e => e)
    expect((err as ApiError).status).toBe(400)
  })

  it('throws ApiError 400 for message over 1000 chars', async () => {
    mockError(400)
    const err = await chat('sess', 'a'.repeat(1001)).catch(e => e)
    expect((err as ApiError).status).toBe(400)
  })

  it('throws ApiError 502 on LLM failure', async () => {
    mockError(502)
    const err = await chat('sess', 'Oi').catch(e => e)
    expect((err as ApiError).status).toBe(502)
  })
})
