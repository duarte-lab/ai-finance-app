import { render, screen } from "@testing-library/react";
import Home from "@/app/page";

describe("Home", () => {
  it("renders menu cards with links to feature actions", () => {
    render(<Home />);

    expect(
      screen.getByRole("link", { name: /dashboard/i }),
    ).toHaveAttribute("href", "/dashboard");

    expect(
      screen.getByRole("link", { name: /contas/i }),
    ).toHaveAttribute("href", "/accounts");

    expect(
      screen.getByRole("link", { name: /fechamento/i }),
    ).toHaveAttribute("href", "/closing");

    expect(screen.getByRole("link", { name: /pessoas/i })).toHaveAttribute("href", "/people");
  });
});
