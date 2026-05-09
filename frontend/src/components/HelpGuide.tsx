const commands = [
  {
    action: 'Adicionar reparo',
    example: 'Preciso trocar a lâmpada do corredor',
    description: 'O agente infere categoria e ferramentas necessárias e salva o reparo.',
  },
  {
    action: 'Adicionar vários reparos de uma vez',
    example: 'Preciso trocar a lâmpada, fixar o quadro da sala e limpar o ralo',
    description: 'O agente detecta múltiplos reparos na mesma mensagem e salva todos de uma vez.',
  },
  {
    action: 'Corrigir reparo',
    example: 'A lâmpada é na cozinha, não no corredor',
    description: 'O agente identifica o reparo pelo contexto e atualiza a descrição ou categoria.',
  },
  {
    action: 'Marcar como concluído',
    example: 'Terminei o quadro da sala',
    description: 'O agente identifica o reparo pelo contexto e marca como concluído.',
  },
  {
    action: 'Desfazer conclusão',
    example: 'Errei, ainda não terminei o ralo',
    description: 'O agente reabre o reparo e ele volta para a lista de pendentes.',
  },
  {
    action: 'Remover reparo',
    example: 'Remove a tarefa do ralo',
    description: 'O agente identifica e remove o reparo da lista.',
  },
  {
    action: 'Listar reparos',
    example: 'Quais reparos tenho pendentes?',
    description: 'O agente lista todos os reparos pendentes da sessão.',
  },
  {
    action: 'Próximo a executar',
    example: 'O que faço primeiro?',
    description: 'O agente sugere o próximo reparo com base no kit de ferramentas.',
  },
  {
    action: 'Gerar plano',
    example: 'Organize minha lista de reparos',
    description: 'O agente agrupa os reparos por kit de ferramentas para minimizar trocas.',
  },
  {
    action: 'Lista de compras',
    example: 'O que preciso comprar para fazer os reparos?',
    description: 'O agente consolida os materiais de todos os reparos pendentes por kit, sem repetição.',
  },
  {
    action: 'Estimar tempo',
    example: 'Quanto tempo vou levar para terminar tudo?',
    description: 'O agente estima o tempo de cada reparo e o total, considerando preparação e limpeza.',
  },
  {
    action: 'Resumo da sessão',
    example: 'Como está minha lista hoje?',
    description: 'O agente mostra quantos reparos foram concluídos, quantos estão pendentes e o que vem a seguir.',
  },
]

export function HelpGuide() {
  return (
    <div className="flex flex-col gap-5">
      <p className="text-sm text-zinc-500">
        O agente entende linguagem natural. Use frases como as abaixo para interagir.
      </p>

      <div className="flex flex-col gap-2">
        {commands.map(cmd => (
          <div key={cmd.action} className="rounded-lg border border-zinc-800 bg-zinc-900 px-4 py-3">
            <p className="text-sm font-medium text-zinc-200">{cmd.action}</p>
            <p className="mt-1.5 rounded-md border border-zinc-700 bg-zinc-800 px-3 py-1.5 text-xs font-mono text-zinc-400">
              &ldquo;{cmd.example}&rdquo;
            </p>
            <p className="mt-1.5 text-xs text-zinc-500">{cmd.description}</p>
          </div>
        ))}
      </div>

      <div className="flex flex-col gap-2">
        <div className="rounded-lg border border-zinc-800 bg-zinc-900 px-4 py-3">
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-1">Dica</p>
          <p className="text-xs text-zinc-500">
            Reparos adicionados pelo formulário não têm ferramentas inferidas. Para aproveitar o agrupamento por kit, adicione via chat.
          </p>
        </div>
        <div className="rounded-lg border border-zinc-800 bg-zinc-900 px-4 py-3">
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-1">Dica</p>
          <p className="text-xs text-zinc-500">
            Se sua mensagem for ambígua, o agente vai pedir mais detalhes antes de salvar o reparo.
          </p>
        </div>
      </div>
    </div>
  )
}
