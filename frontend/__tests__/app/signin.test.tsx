import { fireEvent, render, screen } from "@testing-library/react";
import SignInPage from "@/app/auth/signin/page";

const signInMock = jest.fn();

jest.mock("next-auth/react", () => ({
  signIn: (...args: unknown[]) => signInMock(...args),
}));

jest.mock("next/navigation", () => ({
  useSearchParams: () => ({
    get: (key: string) => (key === "callbackUrl" ? "/dashboard" : null),
  }),
}));

describe("SignInPage", () => {
  beforeEach(() => {
    signInMock.mockClear();
  });

  it("starts google sign in with the callback url from the query string", () => {
    render(<SignInPage />);

    fireEvent.click(screen.getByRole("button", { name: /entrar com google/i }));

    expect(signInMock).toHaveBeenCalledWith("google", { callbackUrl: "/dashboard" });
  });
});