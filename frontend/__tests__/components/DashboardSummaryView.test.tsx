import { render, screen } from "@testing-library/react";
import { DashboardSummaryView } from "@/components/DashboardSummaryView";

describe("DashboardSummaryView", () => {
  it("renders cards and chart percentages", () => {
    render(
      <DashboardSummaryView
        initialSummary={{
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
        }}
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

    expect(screen.getByText(/dashboard financeiro/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/mês e ano selecionados/i)).toHaveTextContent("05/2026");
    expect(screen.getByText(/75\s*%/)).toBeInTheDocument();
    expect(screen.getByText(/25\s*%/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /mês anterior/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /mês próximo/i })).toBeInTheDocument();
    expect(screen.getByLabelText("barra-pago")).toBeInTheDocument();
    expect(screen.getByLabelText("barra-pendente")).toBeInTheDocument();
    expect(screen.getByText(/alertas de vencimento/i)).toBeInTheDocument();
    expect(screen.getByText(/a conta 'rent' vence hoje/i)).toBeInTheDocument();
  });
});
