import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { AccountsList } from "@/components/AccountsList";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  getAccounts: jest.fn(),
  markAccountAsPaid: jest.fn(),
  createAccount: jest.fn(),
  getPeople: jest.fn(),
  createPerson: jest.fn(),
  updateAccount: jest.fn(),
  updateAccountDivisionParticipation: jest.fn(),
}));

describe("AccountsList", () => {
  const initialAccounts = [
    {
      id: "account-1",
      name: "Rent",
      amount: 1800,
      dueDate: "2026-05-10T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: false,
    },
  ];

  beforeEach(() => {
    (api.getPeople as jest.Mock).mockResolvedValue([]);
    (api.createAccount as jest.Mock).mockReset();
  });

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
      createdAtUtc: "2026-05-01T00:00:00Z",
      paid: false,
      participatesInDivision: false,
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.change(screen.getByLabelText("Nome da conta"), { target: { value: "Water" } });
    fireEvent.change(screen.getByLabelText("Valor da conta"), { target: { value: "90" } });
    fireEvent.change(screen.getByLabelText("Data de vencimento"), { target: { value: "2026-05-22" } });
    fireEvent.click(screen.getByRole("button", { name: "Criar conta" }));

    await waitFor(() => {
      expect(api.createAccount).toHaveBeenCalledWith({
        name: "Water",
        amount: 90,
        dueDate: "2026-05-22T00:00:00.000Z",
        participatesInDivision: false,
      });
      expect(screen.getByText("Water")).toBeInTheDocument();
    });
  });

  it("creates a person", async () => {
    (api.createPerson as jest.Mock).mockResolvedValue({
      id: "person-1",
      name: "Ana",
      createdAtUtc: "2026-05-01T00:00:00Z",
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.change(screen.getByLabelText("Nome da pessoa"), { target: { value: "Ana" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar pessoa" }));

    await waitFor(() => {
      expect(api.createPerson).toHaveBeenCalledWith("Ana");
      expect(screen.getByText("1 pessoa(s) cadastrada(s)")).toBeInTheDocument();
    });
  });

  it("marks account as participant in division", async () => {
    (api.updateAccountDivisionParticipation as jest.Mock).mockResolvedValue({
      ...initialAccounts[0],
      participatesInDivision: true,
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.click(screen.getByRole("button", { name: "Marcar na divisao" }));

    await waitFor(() => {
      expect(api.updateAccountDivisionParticipation).toHaveBeenCalledWith("account-1", true);
      expect(screen.getByText(/divisao mensal:\s*participa/i)).toBeInTheDocument();
    });
  });

  it("edits an account", async () => {
    (api.updateAccount as jest.Mock).mockResolvedValue({
      ...initialAccounts[0],
      name: "Rent updated",
    });

    render(<AccountsList initialAccounts={initialAccounts} initialYear={2026} initialMonth={5} />);

    fireEvent.click(screen.getByRole("button", { name: "Editar" }));
    fireEvent.change(screen.getByLabelText("Editar nome da conta"), {
      target: { value: "Rent updated" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Salvar" }));

    await waitFor(() => {
      expect(api.updateAccount).toHaveBeenCalledTimes(1);
      expect(screen.getByText("Rent updated")).toBeInTheDocument();
    });
  });

});
