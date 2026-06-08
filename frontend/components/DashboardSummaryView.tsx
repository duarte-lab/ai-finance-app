"use client";

import { useState } from "react";
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from "chart.js";
import { Bar, Line, Pie } from "react-chartjs-2";
import { DashboardSummary, DueNotification, getDashboardSummary } from "@/services/api";
import { handleApiError } from "@/lib/client-auth";
import { MonthNavigation } from "@/components/MonthNavigation";

ChartJS.register(
  ArcElement,
  BarElement,
  CategoryScale,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
);

interface DashboardSummaryViewProps {
  initialSummary: DashboardSummary;
  initialNotifications: DueNotification[];
  initialYear: number;
  initialMonth: number;
  token?: string;
}

function currency(value: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function DashboardSummaryView({
  initialSummary,
  initialNotifications,
  initialYear,
  initialMonth,
  token,
}: DashboardSummaryViewProps) {
  const [summary, setSummary] = useState<DashboardSummary>(initialSummary);
  const [notifications] = useState<DueNotification[]>(initialNotifications);
  const [year, setYear] = useState<number>(initialYear);
  const [month, setMonth] = useState<number>(initialMonth);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function applyFilter(nextYear = year, nextMonth = month) {
    setIsLoading(true);
    setError(null);

    try {
      const nextSummary = await getDashboardSummary({ year: nextYear, month: nextMonth }, token);
      setSummary(nextSummary);
    } catch (err) {
      const message = handleApiError(err, "Failed to filter dashboard.");
      if (message) setError(message);
    } finally {
      setIsLoading(false);
    }
  }

  const pieData = {
    labels: summary.chart.map((p) => p.label),
    datasets: [
      {
        data: summary.chart.map((p) => p.amount),
        backgroundColor: ["#22c55e", "#f59e0b"],
        borderWidth: 1,
      },
    ],
  };

  const lineData = {
    labels: summary.paidSeries.map((p) => p.label),
    datasets: [
      {
        label: "Pago (R$)",
        data: summary.paidSeries.map((p) => p.amount),
        borderColor: "#22c55e",
        backgroundColor: "rgba(34,197,94,0.15)",
        tension: 0.3,
        fill: true,
      },
    ],
  };

  const barData = {
    labels: summary.lastSixMonths.map(
      (p) => `${String(p.month).padStart(2, "0")}/${p.year}`,
    ),
    datasets: [
      {
        label: "Total (R$)",
        data: summary.lastSixMonths.map((p) => p.totalAmount),
        backgroundColor: "#6366f1",
      },
    ],
  };

  const chartOptions = { responsive: true, maintainAspectRatio: true };

  return (
    <main className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-6">
      <section className="w-full rounded-2xl border border-slate-200 bg-white p-4">
        <MonthNavigation
          year={year}
          month={month}
          onChange={(nextYear, nextMonth) => {
            setYear(nextYear);
            setMonth(nextMonth);
            void applyFilter(nextYear, nextMonth);
          }}
          ariaLabel="Navegacao mensal do dashboard"
        />

        {error && <p className="mt-3 rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}
      </section>

      {notifications.length > 0 && (
        <section className="rounded-2xl border border-amber-200 bg-amber-50 p-6">
          <h2 className="text-lg font-semibold text-amber-900">Alertas de vencimento</h2>
          <p className="mt-1 text-sm text-amber-800">
            {notifications.length} conta(s) com vencimento hoje ou nos proximos 3 dias.
          </p>

          <ul className="mt-4 space-y-2">
            {notifications.map((item) => (
              <li
                key={item.accountId}
                className="rounded-md border border-amber-200 bg-white p-3 text-sm text-slate-800"
              >
                <p className="font-medium">{item.accountName}</p>
                <p>{item.message}</p>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section className="rounded-2xl border border-slate-200 bg-white p-6">
        <h1 className="text-2xl font-semibold text-slate-900">Painel de Controle</h1>
        <p className="mt-1 text-sm text-slate-600">
          Resumo de {String(summary.month).padStart(2, "0")}/{summary.year}
        </p>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <p className="text-sm text-slate-600">Total do mes</p>
          <p className="mt-2 text-2xl font-semibold text-slate-900">{currency(summary.totalAmount)}</p>
          <p className="mt-2 text-xs text-slate-500">{summary.totalCount} conta(s)</p>
        </article>

        <article className="rounded-2xl border border-green-200 bg-white p-5">
          <p className="text-sm text-slate-600">Contas pagas</p>
          <p className="mt-2 text-2xl font-semibold text-green-700">{currency(summary.paidAmount)}</p>
          <p className="mt-2 text-xs text-slate-500">{summary.paidCount} conta(s)</p>
        </article>

        <article className="rounded-2xl border border-amber-200 bg-white p-5">
          <p className="text-sm text-slate-600">Contas pendentes</p>
          <p className="mt-2 text-2xl font-semibold text-amber-700">{currency(summary.pendingAmount)}</p>
          <p className="mt-2 text-xs text-slate-500">{summary.pendingCount} conta(s)</p>
        </article>
      </section>

      <section
        aria-label="graficos-mensais"
        className="grid gap-6 rounded-2xl border border-slate-200 bg-white p-6 md:grid-cols-2"
      >
        <div>
          <h2 className="mb-4 text-base font-semibold text-slate-900">Pago vs Pendente</h2>
          <Pie data={pieData} options={chartOptions} aria-label="grafico-pizza" />
        </div>

        <div>
          <h2 className="mb-4 text-base font-semibold text-slate-900">Contas pagas no mes</h2>
          <Line data={lineData} options={chartOptions} aria-label="grafico-linha" />
        </div>
      </section>

      <section
        aria-label="grafico-ultimos-6-meses"
        className="rounded-2xl border border-slate-200 bg-white p-6"
      >
        <h2 className="mb-4 text-base font-semibold text-slate-900">Totais dos ultimos 6 meses</h2>
        <Bar
          data={barData}
          options={{ ...chartOptions, plugins: { legend: { display: false } } }}
          aria-label="grafico-barras"
        />
      </section>

      {isLoading && (
        <p className="text-center text-sm text-slate-500">Carregando...</p>
      )}
    </main>
  );
}
