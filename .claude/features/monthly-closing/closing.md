# Feature: Monthly Closing

## Objetivo
Fechar o mês e dividir despesas.

## Requisitos
- Selecionar contas do mês
- Escolher participantes
- Calcular divisão

## Regras
- Apenas contas não pagas entram
- Divisão igualitária inicialmente
- Registrar histórico

## Backend
- Endpoint POST /closing
- Criar entidade MonthlyClosing

## Resultado esperado
- Total do mês
- Valor por pessoa

## Testes
- Fechamento com múltiplas contas
- Fechamento sem contas (erro)