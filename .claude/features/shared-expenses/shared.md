# Feature: Shared Expenses

## Objetivo
Associar pessoas participantes a contas.

## Requisitos
- Cadastro de pessoas
- Associação com contas

## Backend
- Entidade Person
- Relação N:N com Account

## Regras
- Pessoa não pode participar duas vezes
- Participantes devem existir na lista de pessoas cadastradas

## Testes
- Associação correta de participantes na conta
- Validação de participantes duplicados
- Validação de participantes inexistentes