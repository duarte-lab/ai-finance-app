# Feature: Person

## Objetivo 
Manter o cadastro de pessoas participantes usado no fechamento mensal.

# Requisitos
- Criar Pessoa
- Listar Pessoa
- Excluir Pessoa

# Campos
- Data de criação
- Nome
- Data de exclusão

# Regras
- A data de criação é imutável
- O nome deve ter no máximo 50 caracteres
- A data de exclusão deve respeitar 30 dias antes de remover o registro da base
- Pessoas cadastradas ficam disponiveis para selecao no fechamento mensal

## Backend
- CRUD completo
- DTOs obrigatórios
- Entidade Person
- Endpoint GET /api/people
- Endpoint POST /api/people

## Frontend
- Criar página específica
- Lista de Pessoas
- Botão para adicionar Pessoa
- Botão para excluir Pessoa
- Remover componentes da tela de Contas, caso exista ainda
- Adicionar atalho na tela Home

## Testes
- Criacao de pessoa
- Retorno da lista de pessoas