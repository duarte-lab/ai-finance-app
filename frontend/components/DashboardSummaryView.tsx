"use client";

import { useState } from "react";
import { DashboardSummary, getDashboardSummary } from "@/services/api";

interface DashboardSummaryViewProps {
  initialSummary: DashboardSummary;
  initialYear: number;
  initialMonth: number;
}

function currency(value: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

function getBarPercentage(value: number, total: number): number {
  if (total <= 0) {
    return 0;
  }

  return Math.round((value / total) * 100);
}

export function DashboardSummaryView({
  initialSummary,
  initialYear,
  initialMonth,
}: DashboardSummaryViewProps) {
  const [summary, setSummary] = useState<DashboardSummary>(initialSummary);
  const [year, setYear] = useState<number>(initialYear);
  const [month, setMonth] = useState<number>(initialMonth);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function applyFilter() {
    setIsLoading(true);
    setError(null);

    try {
      const nextSummary = await getDashboardSummary({ year, month });
      setSummary(nextSummary);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to filter dashboard.");
    } finally {
      setIsLoading(false);
    }
  }

  const paidPercentage = getBarPercentage(summary.paidAmount, summary.totalAmount);
  const pendingPercentage = getBarPercentage(summary.pendingAmount, summary.totalAmount);

  return (
    <main className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-6">
      <section className="rounded-2xl border border-slate-200 bg-white p-6">
        <h1 className="text-2xl font-semibold text-slate-900">Dashboard financeiro</h1>
        <p className="mt-1 text-sm text-slate-600">
          Resumo de {String(summary.month).padStart(2, "0")}/{summary.year}
        </p>

        <div className="mt-4 flex flex-wrap items-end gap-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Ano
            <input
              aria-label="Ano do dashboard"
              type="number"
              min={1}
              value={year}
              onChange={(e) => setYear(Number(e.target.value))}
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Mes
            <input
              aria-label="Mes do dashboard"
              type="number"
              min={1}
              max={12}
              value={month}
              onChange={(e) => setMonth(Number(e.target.value))}
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>

          <button
            type="button"
            onClick={applyFilter}
            disabled={isLoading}
            className="rounded-md bg-slate-900 px-4 py-2 text-white disabled:opacity-60"
          >
            {isLoading ? "Filtrando..." : "Filtrar"}
          </button>
        </div>

        {error && <p className="mt-3 rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}
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

      <section className="rounded-2xl border border-slate-200 bg-white p-6">
        <h2 className="text-lg font-semibold text-slate-900">Grafico simples</h2>
        <p className="mt-1 text-sm text-slate-600">Distribuicao entre pago e pendente no mes.</p>

        <div className="mt-5 flex flex-col gap-4">
          <div>
            <div className="mb-1 flex justify-between text-sm">
              <span className="font-medium text-green-700">Pago</span>
              <span className="text-slate-700">{paidPercentage}%</span>
            </div>
            <div className="h-3 rounded-full bg-slate-100">
              <div
                className="h-3 rounded-full bg-green-500"
                style={{ width: `${paidPercentage}%` }}
                aria-label="barra-pago"
              />
            </div>
          </div>

          <div>
            <div className="mb-1 flex justify-between text-sm">
              <span className="font-medium text-amber-700">Pendente</span>
              <span className="text-slate-700">{pendingPercentage}%</span>
            </div>
            <div className="h-3 rounded-full bg-slate-100">
              <div
                className="h-3 rounded-full bg-amber-500"
                style={{ width: `${pendingPercentage}%` }}
                aria-label="barra-pendente"
              />
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}
