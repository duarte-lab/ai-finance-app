import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MonthlyClosingView } from "@/components/MonthlyClosingView";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  getAccounts: jest.fn(),
  getMonthlyClosing: jest.fn(),
  createMonthlyClosing: jest.fn(),
  reopenMonthlyClosing: jest.fn(),
}));

describe("MonthlyClosingView", () => {
  const token = "backend-token";

  const initialAccounts = [
    {
      id: "account-1",
      name: "Rent",
      amount: 1000,
      dueDate: "2026-05-10T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: true,
    },
    {
      id: "account-2",
      name: "Internet",
      amount: 500,
      dueDate: "2026-05-11T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: false,
    },
  ];

  const initialPeople = [
    {
      id: "person-1",
      name: "Ana",
      type: "Owner",
      createdAtUtc: "2026-04-01T00:00:00Z",
    },
    {
      id: "person-2",
      name: "Bruno",
      type: "Guest",
      createdAtUtc: "2026-04-01T00:00:00Z",
    },
  ];

  beforeEach(() => {
    jest.clearAllMocks();
    (api.getMonthlyClosing as jest.Mock).mockResolvedValue(null);
    (api.getAccounts as jest.Mock).mockResolvedValue([]);
  });

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
      isReopened: false,
      reopenedAtUtc: null,
      participants: ["Ana", "Bruno"],
    });
    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialPeople={initialPeople}
        initialClosing={null}
        initialYear={2026}
        initialMonth={5}
        token={token}
      />,
    );

    fireEvent.click(screen.getByLabelText(/selecionar conta internet/i));
    fireEvent.click(screen.getByRole("button", { name: /fechar mes/i }));

    await waitFor(() => {
      expect(api.createMonthlyClosing).toHaveBeenCalledWith({
        year: 2026,
        month: 5,
        accountIds: ["account-1", "account-2"],
        participants: ["Ana", "Bruno"],
      }, token);
      expect(screen.getByText(/resultado do fechamento/i)).toBeInTheDocument();
      expect(screen.getByText(/mes fechado com sucesso/i)).toBeInTheDocument();
    });
  });

  it("reopens monthly closing and renders reopened status", async () => {
    (api.reopenMonthlyClosing as jest.Mock).mockResolvedValue({
      id: "closing-1",
      year: 2026,
      month: 5,
      totalAmount: 1500,
      amountPerPerson: 750,
      accountCount: 2,
      participantCount: 2,
      closedAtUtc: "2026-05-20T00:00:00Z",
      isReopened: true,
      reopenedAtUtc: "2026-05-25T00:00:00Z",
      participants: ["Ana", "Bruno"],
    });
    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialPeople={initialPeople}
        initialClosing={{
          id: "closing-1",
          year: 2026,
          month: 5,
          totalAmount: 1500,
          amountPerPerson: 750,
          accountCount: 2,
          participantCount: 2,
          closedAtUtc: "2026-05-20T00:00:00Z",
          isReopened: false,
          reopenedAtUtc: null,
          participants: ["Ana", "Bruno"],
        }}
        initialYear={2026}
        initialMonth={5}
        token={token}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /reabrir mes/i }));

    await waitFor(() => {
      expect(api.reopenMonthlyClosing).toHaveBeenCalledWith({ year: 2026, month: 5 }, token);
      expect(screen.getByText(/mes reaberto com sucesso/i)).toBeInTheDocument();
    });
  });

  it("filters month accounts", async () => {
    (api.getAccounts as jest.Mock).mockResolvedValue([
      {
        id: "account-3",
        name: "Water",
        amount: 120,
        dueDate: "2026-06-03T00:00:00Z",
        createdAtUtc: "2026-04-01T00:00:00Z",
        paid: true,
        participatesInDivision: false,
      },
    ]);

    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialPeople={initialPeople}
        initialClosing={null}
        initialYear={2026}
        initialMonth={5}
        token={token}
      />,
    );

    fireEvent.change(screen.getByLabelText(/mes do fechamento/i), {
      target: { value: "6" },
    });
    fireEvent.click(screen.getByRole("button", { name: /buscar contas/i }));

    await waitFor(() => {
      expect(api.getAccounts).toHaveBeenLastCalledWith({ year: 2026, month: 6 }, token);
      expect(api.getMonthlyClosing).toHaveBeenLastCalledWith(2026, 6, token);
      expect(screen.getByText("Water")).toBeInTheDocument();
      expect(screen.getByText("Pago")).toBeInTheDocument();
    });
  });

  it("renders paid accounts from initial month list", () => {
    render(
      <MonthlyClosingView
        initialAccounts={[
          {
            id: "account-paid",
            name: "Security",
            amount: 150,
            dueDate: "2026-05-23T00:00:00Z",
            createdAtUtc: "2026-04-01T00:00:00Z",
            paid: true,
            participatesInDivision: true,
          },
        ]}
        initialPeople={initialPeople}
        initialClosing={null}
        initialYear={2026}
        initialMonth={5}
        token={token}
      />,
    );

    expect(screen.getByText("Security")).toBeInTheDocument();
    expect(screen.getByText("Pago")).toBeInTheDocument();
  });

  it("disables closing when the selected month is already closed", () => {
    render(
      <MonthlyClosingView
        initialAccounts={initialAccounts}
        initialPeople={initialPeople}
        initialClosing={{
          id: "closing-1",
          year: 2026,
          month: 5,
          totalAmount: 1500,
          amountPerPerson: 750,
          accountCount: 2,
          participantCount: 2,
          closedAtUtc: "2026-05-20T00:00:00Z",
          isReopened: false,
          reopenedAtUtc: null,
          participants: ["Ana", "Bruno"],
        }}
        initialYear={2026}
        initialMonth={5}
        token={token}
      />,
    );

    expect(screen.getByRole("button", { name: /mes ja fechado/i })).toBeDisabled();
    expect(screen.getByText("Ana:")).toBeInTheDocument();
    expect(screen.getByText("Bruno:")).toBeInTheDocument();
  });
});
