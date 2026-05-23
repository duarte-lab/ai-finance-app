# Feature: Accounts

## Objetivo
Gerenciar contas domésticas.

## Requisitos
- Criar conta
- Listar contas
- Marcar como paga
- Filtrar por mês

## Campos
- Nome
- Valor
- Data de vencimento
- Pago (bool)

## Regras
- Datas devem ser UTC
- Não permitir valor negativo
- Permitir data retroativa ao criar

## Backend
- CRUD completo
- DTOs obrigatórios

## Frontend
- Criar página específica
- Lista de contas
- Botão "pagar"

## Testes
- Criar conta
- Marcar como paga