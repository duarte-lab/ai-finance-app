export type MenuItem = {
  title: string;
  description: string;
  href: string;
  badge?: string;
  icon?: "chart" | "wallet" | "calendar" | "users";
};

export type MenuSection = {
  title: string;
  description: string;
  href?: string;
  icon: "overview" | "finance";
  items: MenuItem[];
};

export const drawerMenuSections: MenuSection[] = [
  {
    title: "Visao geral",
    description: "Acesso rapido ao inicio e acompanhamento mensal",
    href: "/",
    icon: "overview",
    items: [
      {
        title: "Dashboard",
        description: "Visao geral financeira do mes",
        href: "/dashboard",
        icon: "chart",
      },
    ],
  },
  {
    title: "Gestao financeira",
    description: "Operacoes do dia a dia da casa",
    icon: "finance",
    items: [
      {
        title: "Contas",
        description: "Gestao de contas domesticas",
        href: "/accounts",
        icon: "wallet",
      },
      {
        title: "Fechamento",
        description: "Fechamento mensal e divisao de despesas",
        href: "/closing",
        icon: "calendar",
      },
      {
        title: "Pessoas",
        description: "Gestao de pessoas da casa",
        href: "/people",
        icon: "users",
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
