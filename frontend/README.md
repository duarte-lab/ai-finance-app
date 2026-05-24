# Frontend

Frontend em Next.js para o sistema de controle de contas domesticas.

## Funcionalidades

- Tela inicial com atalhos para os fluxos principais
- Dashboard com resumo financeiro mensal e alertas de vencimento
- Gestao de contas com criacao, edicao, filtro por mes e marcacao de pagamento
- Fechamento mensal com selecao de participantes, consulta do fechamento ativo e reabertura

## Rotas

- /
- /dashboard
- /accounts
- /closing

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
- Em ambiente integrado, a stack pode ser iniciada com docker compose na raiz do repositorio
- Os testes usam Jest + Testing Library

## Integracao com a API

- Dashboard: resumo mensal e notificacoes de vencimento
- Accounts: operacoes de contas, pessoas e marcacao para divisao mensal
- Closing: consulta, criacao e reabertura do fechamento mensal

## Observacoes

- Nao existe fluxo de autenticacao implementado na interface
- O projeto usa App Router
- Os tipos de consumo da API ficam em services/api.ts
