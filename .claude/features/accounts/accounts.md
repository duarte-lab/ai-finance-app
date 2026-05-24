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
- Participa da divisão (bool)
- Pago (bool)
- Data de criação (não alterável)

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
- Permitir editar

## Testes
- Criar conta
- Marcar como paga
- Marcar como participante da divisão