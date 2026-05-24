# Feature: Monthly Closing

## Objetivo
Fechar o mês e dividir despesas.

## Requisitos
- Selecionar contas do mês
- Escolher participantes
- Calcular divisão

## Regras
- Todas as contas marcadas como participantes da divisão entram na conta
- As contas que não foram marcadas devem aparecer para dar a opção de incluir na divisão
- Divisão igualitária inicialmente
- Registrar histórico
- Marcar todas as contas como pagas ao fechar o mês.
- Permitir reabertura do mês

## Backend
- Endpoint POST /closing
- Criar entidade MonthlyClosing

## Resultado esperado
- Total do mês
- Valor por pessoa

## Testes
- Fechamento com múltiplas contas
- Fechamento sem contas (erro)
- Fechamentos com contas não pagas
- Fechamento com contas que participam e que não participam da divisão