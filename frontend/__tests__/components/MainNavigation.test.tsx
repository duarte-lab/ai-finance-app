import { render, screen } from "@testing-library/react";
import { MainNavigation } from "@/components/MainNavigation";

jest.mock("next-auth/react", () => ({
  useSession: () => ({ data: null }),
  signOut: jest.fn(),
}));

describe("MainNavigation", () => {
  it("renders app brand and session actions", () => {
    render(<MainNavigation />);

    expect(screen.getByRole("link", { name: /ai finance app/i })).toHaveAttribute(
      "href",
      "/",
    );
  });
});
