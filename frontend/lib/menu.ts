export type MenuItem = {
  title: string;
  description: string;
  href: string;
  badge?: string;
};

export const headerMenuItems: MenuItem[] = [
  {
    title: "Inicio",
    description: "Tela inicial do sistema",
    href: "/",
  },
  {
    title: "Dashboard",
    description: "Visao geral financeira do mes",
    href: "/dashboard",
  },
  {
    title: "Contas",
    description: "Gestao de contas domesticas",
    href: "/accounts",
  },
];

export const homeFeatureMenuItems: MenuItem[] = [
  {
    title: "Dashboard financeiro",
    description: "Acompanhe total do mes, pagos e pendentes",
    href: "/dashboard",
    badge: "Novo",
  },
  {
    title: "Visao geral de contas",
    description: "Lista todas as contas do mes atual",
    href: "/accounts",
    badge: "Ativo",
  },
  {
    title: "Criar nova conta",
    description: "Abre a secao de cadastro de conta",
    href: "/accounts#nova-conta",
    badge: "Ativo",
  },
  {
    title: "Filtrar por mes",
    description: "Abre o filtro por ano e mes",
    href: "/accounts#filtro-mensal",
    badge: "Ativo",
  },
  {
    title: "Marcar conta como paga",
    description: "Acessa a listagem para atualizar o status",
    href: "/accounts#lista-contas",
    badge: "Ativo",
  },
];
