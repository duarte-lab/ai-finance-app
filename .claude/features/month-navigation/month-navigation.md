# Feature: Month Navigation

## Objetivo
Criar um componente de navegação mensal independente, reutilizável entre as telas, para padronizar a navegação de mês/ano que o usuário está visualizando.

## Requisitos
- Componente independente e compartilhado entre Dashboard, Accounts e Monthly Closing
- Deve existir como componente único e desacoplado dos elementos específicos de cada tela
- Exibir mês e ano atualmente selecionados
- Exibir botão para navegar para o mês anterior
- Exibir botão para navegar para o próximo mês

- Disparar evento de mudança sempre que a navegação de mês for acionada
- Permitir receber mês/ano inicial por propriedade

## Regras
- Navegação deve respeitar UTC
- Exibição de mês/ano deve seguir padrão consistente em toda a aplicação
- Estado exibido deve refletir sempre o mês/ano atualmente ativo na tela
- Componente deve ficar entre a navbar e os elementos específicos de cada tela
- Componente deve ocupar toda a largura horizontal disponível da tela
- Exibir mês e ano no formato MM/YYYY
- O mês/ano exibido deve ser no formato negrito e deve ter um tamanho de fonte maior que os botões
- Exibir botão "Mês Anterior" à esquerda do mês/ano
- Exibir botão "Mês Próximo" à direita do mês/ano
- Botões devem ficar nas extremidades laterais
- Mês/ano deve ficar centralizado no meio da largura do componente, entre os botões
- Ao clicar em "Mês Anterior", navegar para o mês anterior
- Ao clicar em "Mês Próximo", navegar para o próximo mês

## Frontend
- Criar componente reutilizável de navegação mensal
- O elemento deve estar no topo das telas que possuem navegação mensal (Dashboard, Accounts, Monthly Closing)
- O elemento deve ser renderizado entre a navbar e o conteúdo específico da tela
- O elemento deve ocupar largura total e manter mês/ano centralizado com botões nas extremidades
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
- Renderiza o componente no topo, entre navbar e conteúdo específico
- Renderiza navegação em largura total com mês/ano centralizado e botões nas extremidades
