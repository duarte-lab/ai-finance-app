import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { AccountsList } from "@/components/AccountsList";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  getAccounts: jest.fn(),
  markAccountAsPaid: jest.fn(),
  createAccount: jest.fn(),
}));

describe("AccountsList", () => {
  const initialAccounts = [
    {
      id: "account-1",
      name: "Rent",
      amount: 1800,
      dueDate: "2026-05-10T00:00:00Z",
      paid: false,
    },
  ];

  it("shows accounts and marks an account as paid", async () => {
    (api.markAccountAsPaid as jest.Mock).mockResolvedValue({
      ...initialAccounts[0],
      paid: true,
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    expect(screen.getByText("Rent")).toBeInTheDocument();
    expect(screen.getByText("Pendente")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Pagar" }));

    await waitFor(() => {
      expect(api.markAccountAsPaid).toHaveBeenCalledWith("account-1");
      expect(screen.getByText("Pago")).toBeInTheDocument();
    });
  });

  it("applies month filter", async () => {
    (api.getAccounts as jest.Mock).mockResolvedValue([]);

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.change(screen.getByLabelText("Mês"), { target: { value: "6" } });
    fireEvent.click(screen.getByRole("button", { name: "Filtrar" }));

    await waitFor(() => {
      expect(api.getAccounts).toHaveBeenCalledWith({ year: 2026, month: 6 });
    });
  });

  it("creates a new account", async () => {
    (api.createAccount as jest.Mock).mockResolvedValue({
      id: "account-2",
      name: "Water",
      amount: 90,
      dueDate: "2026-05-22T00:00:00Z",
      paid: false,
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.change(screen.getByLabelText("Nome da conta"), { target: { value: "Water" } });
    fireEvent.change(screen.getByLabelText("Valor da conta"), { target: { value: "90" } });
    fireEvent.change(screen.getByLabelText("Data de vencimento"), { target: { value: "2026-05-22" } });
    fireEvent.click(screen.getByRole("button", { name: "Criar conta" }));

    await waitFor(() => {
      expect(api.createAccount).toHaveBeenCalledTimes(1);
      expect(screen.getByText("Water")).toBeInTheDocument();
    });
  });
});
