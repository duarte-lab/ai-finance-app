# AI Finance App

Aplicacao para controle de contas domesticas, pessoas participantes e fechamento mensal.

## Stack

- Backend em .NET 9 com ASP.NET Core Web API
- Frontend em Next.js 16
- MongoDB para persistencia
- Docker e Docker Compose para execucao local

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
- Pessoas ativas ficam disponiveis para o fechamento mensal

## Rotas do frontend

- `/` - Home
- `/dashboard` - Dashboard
- `/accounts` - Contas
- `/closing` - Fechamento mensal
- `/people` - Pessoas

## API principal

- `GET /api/accounts`
- `POST /api/accounts`
- `PUT /api/accounts/{id}`
- `PATCH /api/accounts/{id}/pay`
- `PATCH /api/accounts/{id}/division-participation`
- `GET /api/dashboard/summary`
- `GET /api/notifications/due`
- `GET /api/people`
- `POST /api/people`
- `PUT /api/people/{id}`
- `DELETE /api/people/{id}`
- `GET /closing`
- `POST /closing`
- `POST /closing/reopen`

## Como executar

### Com Docker

```bash
docker compose up --build
```

### Com Docker em desenvolvimento (hot reload, sem rebuild continuo)

Use o compose de desenvolvimento para backend e frontend com recarga automatica.

Primeira subida (gera imagens de dev):

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

Subidas seguintes (sem rebuild):

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --no-build
```

Quando precisar reconstruir:

- alteracao em package.json
- alteracao em .csproj
- alteracao em Dockerfiles

Para limpar volumes de cache (node_modules/.next/nuget) e forcar ambiente limpo:

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml down -v
```

### Backend

```bash
cd backend
dotnet test tests/Application.Tests/Application.Tests.csproj
dotnet test tests/API.IntegrationTests/API.IntegrationTests.csproj
```

### Frontend

```bash
cd frontend
npm install
npm run dev
npm test -- --runInBand
```

## Testes

O projeto possui testes unitarios e de integracao para os fluxos principais do backend, alem de testes de componentes e servicos no frontend.

## Observacoes

- A aplicacao segue Clean Architecture
- A API usa DTOs para entrada e saida
- As regras de tempo usam UTC
- A interface nao possui autenticacao implementada