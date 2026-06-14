import { render, screen } from "@testing-library/react";
import { MainNavigation } from "@/components/MainNavigation";

jest.mock("next-auth/react", () => ({
  useSession: () => ({ data: null }),
  signOut: jest.fn(),
}));

describe("MainNavigation", () => {
  it("renders app brand and drawer trigger", () => {
    const onToggleDrawer = jest.fn();

    render(<MainNavigation onToggleDrawer={onToggleDrawer} />);

    expect(screen.getByRole("link", { name: /ai finance app/i })).toHaveAttribute(
      "href",
      "/",
    );

    screen.getByRole("button", { name: /abrir menu lateral/i }).click();
    expect(onToggleDrawer).toHaveBeenCalledTimes(1);
  });
});
