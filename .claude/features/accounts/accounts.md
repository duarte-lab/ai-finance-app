# Feature: Accounts

## Objetivo
Gerenciar contas domésticas.

## Requisitos
- Criar conta
- Listar contas
- Excluir conta
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
- Conta só pode ser excluída se o mês não estiver fechado

## Backend
- CRUD completo
- DTOs obrigatórios

## Frontend
- Criar página específica
- Lista de contas
- Navegação de mês deve usar componente independente e reutilizável
- Exibir botão "..." por conta com todas as ações disponíveis
- Ações da conta devem ficar dentro do menu "..." (ex.: pagar, editar, excluir)
- Permitir editar
- Campos do formulário de inclusão devem ter placeholders
- Campo de data deve iniciar com a data atual
- Ao editar a data, manter a data selecionada pelo usuário

## Testes
- Criar conta
- Excluir conta
- Impedir exclusão de conta quando o mês estiver fechado
- Navegação mensal utiliza componente compartilhado com as telas de dashboard e monthly closing
- Marcar como paga
- Marcar como participante da divisão
- Exibir placeholders nos campos de inclusão
- Preencher data inicial com a data atual
- Manter data selecionada após modificação no campo de data