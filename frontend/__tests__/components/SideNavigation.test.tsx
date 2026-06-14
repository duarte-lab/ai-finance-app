import { render, screen } from "@testing-library/react";
import { SideNavigation } from "@/components/SideNavigation";

jest.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

describe("SideNavigation", () => {
  it("renders section items and sub-items", () => {
    render(<SideNavigation isOpen={true} onClose={jest.fn()} />);

    expect(screen.getByRole("complementary", { name: /menu lateral/i })).toBeInTheDocument();

    expect(screen.getByRole("link", { name: /visao geral/i })).toHaveAttribute("href", "/");
    expect(screen.getByRole("link", { name: /dashboard/i })).toHaveAttribute(
      "href",
      "/dashboard",
    );
    expect(screen.getByRole("link", { name: /contas/i })).toHaveAttribute("href", "/accounts");
    expect(screen.getByRole("link", { name: /fechamento/i })).toHaveAttribute(
      "href",
      "/closing",
    );
    expect(screen.getByRole("link", { name: /pessoas/i })).toHaveAttribute("href", "/people");
  });
});
