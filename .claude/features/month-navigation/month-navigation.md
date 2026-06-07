# Feature: Month Navigation

## Objetivo
Criar um componente de navegação mensal reutilizável entre as telas.

## Requisitos
- Componente independente e compartilhado entre Dashboard, Accounts e Monthly Closing
- Exibir mês e ano atualmente selecionados
- Exibir botão para navegar para o mês anterior
- Exibir botão para navegar para o próximo mês
- Disparar evento de mudança sempre que a navegação de mês for acionada
- Permitir receber mês/ano inicial por propriedade

## Regras
- Navegação deve respeitar UTC
- Exibição de mês/ano deve seguir padrão consistente em toda a aplicação
- Estado exibido deve refletir sempre o mês/ano atualmente ativo na tela

## Frontend
- Criar componente reutilizável de navegação mensal
- Receber propriedades: referência de mês/ano atual e callback de alteração
- Expor ações de anterior e próximo no próprio componente
- Garantir acessibilidade básica dos botões (labels e foco)

## Integração
- Dashboard deve usar este componente para trocar o mês dos gráficos mensais
- Accounts deve usar este componente para trocar o mês da listagem
- Monthly Closing deve usar este componente para trocar o período do fechamento

## Testes
- Renderiza mês/ano selecionado corretamente
- Clique em anterior altera para o mês anterior
- Clique em próximo altera para o próximo mês
- Dispara callback ao navegar
- Permite reutilização nas telas Dashboard, Accounts e Monthly Closing
