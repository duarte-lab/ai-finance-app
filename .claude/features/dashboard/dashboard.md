# Feature: Dashboard

## Objetivo
Exibir visão geral financeira.

## Requisitos
- O título deve ser Painel de Controle
- Total do mês
- Contas pagas vs pendentes
- Utilizar Chart.js como biblioteca de gráficos
- Campo para selecionar o mês exibido nos gráficos mensais
- Gráfico de pizza com categorias das contas (informações mensais)
- Gráfico de linhas com contas pagas (informações mensais)
- Gráfico de barras com totais dos últimos 6 meses
- Alertas de vencimento na interface

## Backend
- Endpoint GET /api/dashboard/summary
- Aceita query string opcional com year e month
- Retornar dados mensais por categoria para o gráfico de pizza
- Retornar série mensal de contas pagas para o gráfico de linhas
- Retornar totais agregados dos últimos 6 meses para o gráfico de barras

## Frontend
- Cards
- Implementar os gráficos com Chart.js
- Navegação de mês deve usar componente independente e reutilizável
- Campo de seleção de mês para filtrar os gráficos mensais deve usar o componente compartilhado
- Exibir gráfico de pizza ocupando metade da largura da área de gráficos
- Exibir gráfico de linhas ocupando a outra metade da largura da área de gráficos
- Exibir gráfico de barras dos últimos 6 meses abaixo dos gráficos de pizza e linhas
- Lista de notificacoes de vencimento

## Testes
- Dados agregados corretos
- Alteração de mês atualiza os gráficos mensais
- Navegação mensal utiliza componente compartilhado com as telas de accounts e monthly closing
- Gráfico de pizza exibe categorias corretamente
- Gráfico de linhas exibe contas pagas do mês selecionado
- Gráfico de barras exibe os totais dos últimos 6 meses