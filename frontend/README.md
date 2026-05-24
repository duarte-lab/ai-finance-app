# Frontend

Frontend em Next.js 16 para o sistema de controle de contas domesticas.

## Features ativas

### Home

- Tela inicial com atalhos para os fluxos principais
- Menu superior com navegacao para as areas ativas da aplicacao
- URL reflete a pagina atual

### Contas

- Listagem das contas do mes
- Criacao de conta
- Edicao de conta
- Marcacao de conta como paga
- Atualizacao da participacao da conta na divisao mensal
- Filtro por ano e mes

### Dashboard

- Resumo financeiro mensal
- Totais de contas pagas e pendentes
- Grafico/resumo por categoria
- Alertas de vencimento

### Fechamento mensal

- Selecao de contas para fechamento
- Selecao de participantes
- Calculo do valor por pessoa
- Consulta do fechamento ativo do mes
- Reabertura do fechamento
- Refechamento do mesmo mes sem criar documento duplicado no Mongo

### Pessoas

- Listagem de pessoas participantes
- Cadastro de nova pessoa
- Edicao de pessoa
- Exclusao de pessoa
- Exclusao em duas etapas: soft delete e remocao definitiva apos 30 dias

## Rotas

- `/`
- `/dashboard`
- `/accounts`
- `/closing`
- `/people`

## Scripts

```bash
npm run dev
npm run build
npm run start
npm run lint
npm test -- --runInBand
```

## Desenvolvimento

- O frontend espera a API backend disponivel localmente
- Em ambiente integrado, a stack pode ser iniciada com `docker compose` na raiz do repositorio
- Os testes usam Jest + Testing Library

## Integracao com a API

- Dashboard: resumo mensal e notificacoes de vencimento
- Accounts: operacoes de contas e marcacao para divisao mensal
- Closing: consulta, criacao e reabertura do fechamento mensal
- People: listagem, criacao, edicao e exclusao de pessoas

## Observacoes

- Nao existe fluxo de autenticacao implementado na interface
- O projeto usa App Router
- Os tipos de consumo da API ficam em `services/api.ts`
