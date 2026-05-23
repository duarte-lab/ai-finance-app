import { render, screen } from "@testing-library/react";
import { MainNavigation } from "@/components/MainNavigation";

describe("MainNavigation", () => {
  it("renders header menu links", () => {
    render(<MainNavigation />);

    expect(screen.getByRole("link", { name: /ai finance app/i })).toHaveAttribute(
      "href",
      "/",
    );

    expect(screen.getByRole("link", { name: /inicio/i })).toHaveAttribute(
      "href",
      "/",
    );

    expect(screen.getByRole("link", { name: /contas/i })).toHaveAttribute(
      "href",
      "/accounts",
    );
  });
});
