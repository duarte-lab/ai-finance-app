export type MenuItem = {
  title: string;
  description: string;
  href: string;
  badge?: string;
};

export type MenuSection = {
  title: string;
  description: string;
  href?: string;
  items: MenuItem[];
};

export const drawerMenuSections: MenuSection[] = [
  {
    title: "Visao geral",
    description: "Acesso rapido ao inicio e acompanhamento mensal",
    href: "/",
    items: [
      {
        title: "Dashboard",
        description: "Visao geral financeira do mes",
        href: "/dashboard",
      },
    ],
  },
  {
    title: "Gestao financeira",
    description: "Operacoes do dia a dia da casa",
    items: [
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
    ],
  },
];

export const homeFeatureMenuItems: MenuItem[] = [
  ...drawerMenuSections.flatMap((section) =>
    section.items.map((item) => ({
      ...item,
      description:
        item.href === "/dashboard"
          ? "Acompanhe total do mes, pagos e pendentes"
          : item.href === "/accounts"
            ? "Lista e gerencia contas do mes atual"
            : item.href === "/people"
              ? "Visualize e organize as pessoas participantes"
              : item.description,
      badge: "Ativo",
    })),
  ),
];
