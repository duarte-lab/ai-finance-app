import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MonthlyClosingView } from "@/components/MonthlyClosingView";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  getAccounts: jest.fn(),
  createMonthlyClosing: jest.fn(),
}));

describe("MonthlyClosingView", () => {
  const initialAccounts = [
    {
      id: "account-1",
      name: "Rent",
      amount: 1000,
      dueDate: "2026-05-10T00:00:00Z",
      paid: false,
      participants: [],
    },
    {
      id: "account-2",
      name: "Internet",
      amount: 500,
      dueDate: "2026-05-11T00:00:00Z",
      paid: false,
      participants: [],
    },
  ];

  it("creates monthly closing and renders result", async () => {
    (api.createMonthlyClosing as jest.Mock).mockResolvedValue({
      id: "closing-1",
      year: 2026,
      month: 5,
      totalAmount: 1500,
      amountPerPerson: 750,
      accountCount: 2,
      participantCount: 2,
      closedAtUtc: "2026-05-20T00:00:00Z",
    });

    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialYear={2026}
        initialMonth={5}
      />,
    );

    fireEvent.click(screen.getByLabelText(/selecionar conta rent/i));
    fireEvent.click(screen.getByLabelText(/selecionar conta internet/i));
    fireEvent.change(screen.getByLabelText(/participantes do fechamento/i), {
      target: { value: "Ana, Bruno" },
    });
    fireEvent.click(screen.getByRole("button", { name: /fechar mes/i }));

    await waitFor(() => {
      expect(api.createMonthlyClosing).toHaveBeenCalledWith({
        year: 2026,
        month: 5,
        accountIds: ["account-1", "account-2"],
        participants: ["Ana", "Bruno"],
      });
      expect(screen.getByText(/resultado do fechamento/i)).toBeInTheDocument();
    });
  });

  it("filters month accounts", async () => {
    (api.getAccounts as jest.Mock).mockResolvedValue([
      {
        id: "account-3",
        name: "Water",
        amount: 120,
        dueDate: "2026-06-03T00:00:00Z",
        paid: false,
        participants: [],
      },
    ]);

    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialYear={2026}
        initialMonth={5}
      />,
    );

    fireEvent.change(screen.getByLabelText(/mes do fechamento/i), {
      target: { value: "6" },
    });
    fireEvent.click(screen.getByRole("button", { name: /buscar contas/i }));

    await waitFor(() => {
      expect(api.getAccounts).toHaveBeenCalledWith({ year: 2026, month: 6 });
      expect(screen.getByText("Water")).toBeInTheDocument();
    });
  });
});
