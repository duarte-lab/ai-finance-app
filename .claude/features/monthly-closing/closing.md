# Feature: Monthly Closing

## Objetivo
Fechar o mês e dividir despesas.

## Requisitos
- Navegação de mês deve usar componente independente e reutilizável
- Selecionar contas do mês
- Carregar participantes da lista de pessoas
- Calcular divisão

## Regras
- Todas as contas marcadas como participantes da divisão entram na conta
- As contas que não foram marcadas devem aparecer para dar a opção de incluir na divisão
- Divisão igualitária inicialmente
- Registrar o fechamento uma única vez
- Desabilitar o botão de fechamento quando o mês estiver fechado
- Marcar todas as contas como pagas ao fechar o mês
- Permitir reabertura do mês
- Exibir o painel de mês fechado com o calculo da divisão pelas pessoas

## Backend
- Endpoint GET /closing?year={year}&month={month}
- Endpoint POST /closing
- Endpoint POST /closing/reopen
- Criar entidade MonthlyClosing

## Comportamento atual
- O fechamento ativo do mes pode ser consultado antes de tentar fechar novamente
- O fechamento registra participantes selecionados a partir da lista de pessoas
- A reabertura desfaz o fechamento ativo do periodo

## Resultado esperado
- Total do mês
- Valor por pessoa

## Testes
- Fechamento com múltiplas contas
- Fechamento sem contas (erro)
- Fechamentos com contas não pagas
- Fechamento com contas que participam e que não participam da divisão
- Navegação mensal utiliza componente compartilhado com as telas de dashboard e accounts