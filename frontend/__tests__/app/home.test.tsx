import { render, screen } from "@testing-library/react";
import Home from "@/app/page";

describe("Home", () => {
  it("renders menu cards with links to feature actions", () => {
    render(<Home />);

    expect(
      screen.getByRole("link", { name: /dashboard financeiro/i }),
    ).toHaveAttribute("href", "/dashboard");

    expect(
      screen.getByRole("link", { name: /visao geral de contas/i }),
    ).toHaveAttribute("href", "/accounts");

    expect(
      screen.getByRole("link", { name: /criar nova conta/i }),
    ).toHaveAttribute("href", "/accounts#nova-conta");

    expect(
      screen.getByRole("link", { name: /filtrar por mes/i }),
    ).toHaveAttribute("href", "/accounts#filtro-mensal");

    expect(
      screen.getByRole("link", { name: /marcar conta como paga/i }),
    ).toHaveAttribute("href", "/accounts#lista-contas");
  });
});
