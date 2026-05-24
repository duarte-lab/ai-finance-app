# Feature: Shared Expenses

## Objetivo
Manter o cadastro de pessoas participantes usado no fechamento mensal.

## Requisitos
- Cadastro de pessoas
- Listagem de pessoas cadastradas

## Backend
- Entidade Person
- Endpoint GET /api/people
- Endpoint POST /api/people

## Regras
- Nome da pessoa e obrigatorio
- Pessoas cadastradas ficam disponiveis para selecao no fechamento mensal

## Testes
- Criacao de pessoa
- Retorno da lista de pessoas