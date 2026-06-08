import { render, screen } from "@testing-library/react";
import { DashboardSummaryView } from "@/components/DashboardSummaryView";

jest.mock("react-chartjs-2", () => ({
  Pie: ({ "aria-label": label }: { "aria-label"?: string }) => <canvas aria-label={label ?? "pie"} />,
  Line: ({ "aria-label": label }: { "aria-label"?: string }) => <canvas aria-label={label ?? "line"} />,
  Bar: ({ "aria-label": label }: { "aria-label"?: string }) => <canvas aria-label={label ?? "bar"} />,
}));

const baseSummary = {
  year: 2026,
  month: 5,
  totalAmount: 1600,
  paidAmount: 1200,
  pendingAmount: 400,
  totalCount: 4,
  paidCount: 3,
  pendingCount: 1,
  chart: [
    { label: "Paid", amount: 1200, count: 3 },
    { label: "Pending", amount: 400, count: 1 },
  ],
  paidSeries: [
    { label: "2026-05-10", amount: 1200, count: 3 },
  ],
  lastSixMonths: [
    { year: 2025, month: 12, totalAmount: 1000 },
    { year: 2026, month: 1, totalAmount: 1100 },
    { year: 2026, month: 2, totalAmount: 900 },
    { year: 2026, month: 3, totalAmount: 1200 },
    { year: 2026, month: 4, totalAmount: 1400 },
    { year: 2026, month: 5, totalAmount: 1600 },
  ],
};

describe("DashboardSummaryView", () => {
  it("renders title Painel de Controle and summary cards", () => {
    render(
      <DashboardSummaryView
        initialSummary={baseSummary}
        initialNotifications={[]}
        initialYear={2026}
        initialMonth={5}
      />,
    );

    expect(screen.getByText(/painel de controle/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/mês e ano selecionados/i)).toHaveTextContent("05/2026");
    expect(screen.getByRole("button", { name: /mês anterior/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /mês próximo/i })).toBeInTheDocument();
  });

  it("renders Chart.js pie, line and bar charts", () => {
    render(
      <DashboardSummaryView
        initialSummary={baseSummary}
        initialNotifications={[]}
        initialYear={2026}
        initialMonth={5}
      />,
    );

    expect(screen.getByLabelText("grafico-pizza")).toBeInTheDocument();
    expect(screen.getByLabelText("grafico-linha")).toBeInTheDocument();
    expect(screen.getByLabelText("grafico-barras")).toBeInTheDocument();
  });

  it("renders due alerts when notifications are present", () => {
    render(
      <DashboardSummaryView
        initialSummary={baseSummary}
        initialNotifications={[
          {
            accountId: "account-1",
            accountName: "Rent",
            dueDateUtc: "2026-05-23T00:00:00Z",
            daysUntilDue: 0,
            type: "DueToday",
            message: "A conta 'Rent' vence hoje.",
          },
        ]}
        initialYear={2026}
        initialMonth={5}
      />,
    );

    expect(screen.getByText(/alertas de vencimento/i)).toBeInTheDocument();
    expect(screen.getByText(/a conta 'rent' vence hoje/i)).toBeInTheDocument();
  });
});
