import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import SignInPage from "@/app/auth/signin/page";

const signInMock = jest.fn();
const pushMock = jest.fn();

jest.mock("next-auth/react", () => ({
  signIn: (...args: unknown[]) => signInMock(...args),
}));

jest.mock("next/navigation", () => ({
  useSearchParams: () => ({
    get: (key: string) => (key === "callbackUrl" ? "/dashboard" : null),
  }),
  useRouter: () => ({
    push: (...args: unknown[]) => pushMock(...args),
  }),
}));

describe("SignInPage", () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    signInMock.mockClear();
    pushMock.mockClear();
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  afterAll(() => {
    global.fetch = originalFetch;
  });

  it("starts google sign in with the callback url from the query string", () => {
    render(<SignInPage />);

    fireEvent.click(screen.getByRole("button", { name: /entrar com google/i }));

    expect(signInMock).toHaveBeenCalledWith("google", { callbackUrl: "/dashboard" });
  });

  it("submits credentials sign in and redirects to callback url", async () => {
    signInMock.mockResolvedValue({ error: null, url: "/dashboard" });

    render(<SignInPage />);

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "ana@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Senha"), {
      target: { value: "password-123" },
    });
    fireEvent.click(screen.getByRole("button", { name: /entrar com email/i }));

    await waitFor(() => {
      expect(signInMock).toHaveBeenCalledWith("credentials", {
        email: "ana@example.com",
        password: "password-123",
        callbackUrl: "/dashboard",
        redirect: false,
      });
      expect(pushMock).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("registers with email/senha and then signs in", async () => {
    (global.fetch as jest.Mock).mockResolvedValue({ ok: true });
    signInMock.mockResolvedValue({ error: null, url: "/dashboard" });

    render(<SignInPage />);

    fireEvent.click(screen.getByRole("button", { name: /criar nova conta/i }));

    fireEvent.change(screen.getByLabelText("Nome"), {
      target: { value: "Ana" },
    });
    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "ana@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Senha"), {
      target: { value: "password-123" },
    });

    fireEvent.click(screen.getByRole("button", { name: /registrar com email/i }));

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/auth/register"),
        expect.objectContaining({ method: "POST" }),
      );
      expect(signInMock).toHaveBeenCalledWith("credentials", {
        email: "ana@example.com",
        password: "password-123",
        callbackUrl: "/dashboard",
        redirect: false,
      });
      expect(pushMock).toHaveBeenCalledWith("/dashboard");
    });
  });
});