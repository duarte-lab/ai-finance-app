# Feature: Dashboard

## Objetivo
Exibir visão geral financeira.

## Requisitos
- Total do mês
- Contas pagas vs pendentes
- Gráfico simples
- Alertas de vencimento na interface

## Backend
- Endpoint GET /api/dashboard/summary
- Aceita query string opcional com year e month

## Frontend
- Cards
- Gráfico
- Lista de notificacoes de vencimento

## Testes
- Dados agregados corretos