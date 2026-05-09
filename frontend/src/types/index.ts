export interface Repair {
  id: string
  sessionId: string
  description: string
  category: string | null
  tools: string[]
  status: 'pendente' | 'concluido'
  createdAt: string
}

export interface PlanGroup {
  group: number
  kitName: string
  tools: string[]
  repairIds: string[]
}

export interface RepairSummary {
  id: string
  description: string
  category: string | null
  tools: string[] | null
}

export interface ShoppingKit {
  kitName: string
  items: string[]
}

export interface TimeEstimate {
  repairId: string
  description: string
  estimatedMinutes: number
}

export interface PlanResponse {
  reply: string
  groups: PlanGroup[]
}

export interface ChatResponse {
  action: string
  reply: string
  createdRepairs?: Repair[]
  editedRepair?: Repair
  markedDoneId?: string
  reopenedId?: string
  removedId?: string
  planGroups?: PlanGroup[]
  repairs?: RepairSummary[]
  nextRepair?: RepairSummary
  shoppingList?: ShoppingKit[]
  timeEstimates?: TimeEstimate[]
}
