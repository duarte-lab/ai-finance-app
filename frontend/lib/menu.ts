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
  {
    title: "Fechamento",
    description: "Fechamento mensal e divisao de despesas",
    href: "/closing",
  },
  {
    title: "Pessoas",
    description: "Gestao de pessoas da casa",
    href: "/people",
  },
];

export const homeFeatureMenuItems: MenuItem[] = [
  {
    title: "Dashboard",
    description: "Acompanhe total do mes, pagos e pendentes",
    href: "/dashboard",
    badge: "Ativo",
  },
  {
    title: "Contas",
    description: "Lista e gerencia contas do mes atual",
    href: "/accounts",
    badge: "Ativo",
  },
  {
    title: "Fechamento",
    description: "Fechamento mensal e divisao de despesas",
    href: "/closing",
    badge: "Ativo",
  },
  {
    title: "Pessoas",
    description: "Visualize e organize as pessoas participantes",
    href: "/people",
    badge: "Ativo",
  },
];
