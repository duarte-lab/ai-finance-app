"use client";

import { useMemo, useState } from "react";
import {
  Account,
  MonthlyClosingResult,
  createMonthlyClosing,
  getAccounts,
  reopenMonthlyClosing,
} from "@/services/api";

interface MonthlyClosingViewProps {
  initialAccounts: Account[];
  initialYear: number;
  initialMonth: number;
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

function toUnpaid(accounts: Account[]): Account[] {
  return accounts.filter((account) => !account.paid);
}

function defaultSelectedAccountIds(accounts: Account[]): string[] {
  return accounts
    .filter((account) => account.participatesInDivision)
    .map((account) => account.id);
}

export function MonthlyClosingView({
  initialAccounts,
  initialYear,
  initialMonth,
}: MonthlyClosingViewProps) {
  const [year, setYear] = useState(initialYear);
  const [month, setMonth] = useState(initialMonth);
  const [accounts, setAccounts] = useState<Account[]>(toUnpaid(initialAccounts));
  const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>(
    defaultSelectedAccountIds(toUnpaid(initialAccounts)),
  );
  const [participantsText, setParticipantsText] = useState("");
  const [result, setResult] = useState<MonthlyClosingResult | null>(null);
  const [isFiltering, setIsFiltering] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReopening, setIsReopening] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const totalSelected = useMemo(
    () =>
      accounts
        .filter((account) => selectedAccountIds.includes(account.id))
        .reduce((total, account) => total + account.amount, 0),
    [accounts, selectedAccountIds],
  );

  async function applyMonthFilter() {
    setIsFiltering(true);
    setError(null);
    setResult(null);

    try {
      const monthAccounts = await getAccounts({ year, month });
      const unpaid = toUnpaid(monthAccounts);
      setAccounts(unpaid);
      setSelectedAccountIds(defaultSelectedAccountIds(unpaid));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load monthly accounts.");
    } finally {
      setIsFiltering(false);
    }
  }

  function toggleAccountSelection(id: string) {
    setSelectedAccountIds((previous) =>
      previous.includes(id)
        ? previous.filter((item) => item !== id)
        : [...previous, id],
    );
  }

  async function submitClosing(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setResult(null);

    const participants = participantsText
      .split(",")
      .map((name) => name.trim())
      .filter((name) => name.length > 0);

    if (selectedAccountIds.length === 0) {
      setError("Selecione pelo menos uma conta nao paga para fechar o mes.");
      return;
    }

    if (participants.length === 0) {
      setError("Informe ao menos um participante.");
      return;
    }

    setIsSubmitting(true);

    try {
      const payload = await createMonthlyClosing({
        year,
        month,
        accountIds: selectedAccountIds,
        participants,
      });

      setResult(payload);
      const refreshed = await getAccounts({ year, month });
      const unpaid = toUnpaid(refreshed);
      setAccounts(unpaid);
      setSelectedAccountIds(defaultSelectedAccountIds(unpaid));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to close month.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function submitReopen() {
    setIsReopening(true);
    setError(null);
    setResult(null);

    try {
      const payload = await reopenMonthlyClosing({ year, month });
      setResult(payload);

      const refreshed = await getAccounts({ year, month });
      const unpaid = toUnpaid(refreshed);
      setAccounts(unpaid);
      setSelectedAccountIds(defaultSelectedAccountIds(unpaid));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to reopen month.");
    } finally {
      setIsReopening(false);
    }
  }

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6">
      <section className="rounded-2xl border border-slate-200 bg-white p-6">
        <h1 className="text-2xl font-semibold text-slate-900">Fechamento mensal</h1>
        <p className="mt-1 text-sm text-slate-600">
          Selecione as contas nao pagas do mes e divida o total entre os participantes.
        </p>

        <div className="mt-4 flex flex-wrap items-end gap-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Ano
            <input
              aria-label="Ano do fechamento"
              type="number"
              min={1}
              value={year}
              onChange={(event) => setYear(Number(event.target.value))}
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Mes
            <input
              aria-label="Mes do fechamento"
              type="number"
              min={1}
              max={12}
              value={month}
              onChange={(event) => setMonth(Number(event.target.value))}
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>

          <button
            type="button"
            onClick={applyMonthFilter}
            disabled={isFiltering}
            className="rounded-md bg-slate-900 px-4 py-2 text-white disabled:opacity-60"
          >
            {isFiltering ? "Carregando..." : "Buscar contas"}
          </button>
        </div>
      </section>

      {error && <p className="rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <form onSubmit={submitClosing} className="rounded-2xl border border-slate-200 bg-white p-6">
        <h2 className="text-lg font-semibold text-slate-900">Contas elegiveis</h2>
        <p className="mt-1 text-sm text-slate-600">
          Contas marcadas para divisao entram automaticamente. Contas nao marcadas podem ser incluidas opcionalmente.
        </p>

        <div className="mt-4 space-y-3">
          {accounts.length === 0 ? (
            <p className="rounded-md border border-dashed border-slate-300 p-4 text-sm text-slate-600">
              Nenhuma conta nao paga encontrada para esse mes.
            </p>
          ) : (
            accounts.map((account) => (
              <label
                key={account.id}
                className="flex items-center justify-between rounded-md border border-slate-200 p-3"
              >
                <div className="flex items-center gap-3">
                  <input
                    aria-label={`Selecionar conta ${account.name}`}
                    type="checkbox"
                    checked={selectedAccountIds.includes(account.id)}
                    onChange={() => toggleAccountSelection(account.id)}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium text-slate-900">{account.name}</span>
                    <span className="text-xs text-slate-500">
                      {account.participatesInDivision
                        ? "Participa automaticamente da divisao"
                        : "Opcional na divisao"}
                    </span>
                    <span className="text-xs text-slate-500">
                      Vencimento: {new Date(account.dueDate).toISOString().slice(0, 10)}
                    </span>
                  </div>
                </div>

                <span className="text-sm font-medium text-slate-800">
                  {formatCurrency(account.amount)}
                </span>
              </label>
            ))
          )}
        </div>

        <div className="mt-5">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Participantes (separados por virgula)
            <input
              aria-label="Participantes do fechamento"
              value={participantsText}
              onChange={(event) => setParticipantsText(event.target.value)}
              placeholder="Ana, Bruno, Carla"
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
        </div>

        <div className="mt-4 flex items-center justify-between gap-3">
          <p className="text-sm text-slate-600">
            Total selecionado: <strong>{formatCurrency(totalSelected)}</strong>
          </p>

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-blue-700 px-4 py-2 text-white disabled:opacity-60"
          >
            {isSubmitting ? "Fechando..." : "Fechar mes"}
          </button>
        </div>
      </form>

      {result && (
        <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6">
          <h2 className="text-lg font-semibold text-emerald-900">Resultado do fechamento</h2>
          <p className="mt-2 text-sm text-emerald-800">
            Status: <strong>{result.isReopened ? "Mes reaberto" : "Mes fechado"}</strong>
          </p>
          <p className="mt-2 text-sm text-emerald-800">
            Total do mes: <strong>{formatCurrency(result.totalAmount)}</strong>
          </p>
          <p className="text-sm text-emerald-800">
            Valor por pessoa: <strong>{formatCurrency(result.amountPerPerson)}</strong>
          </p>
        </section>
      )}

      <section className="rounded-2xl border border-slate-200 bg-white p-6">
        <h2 className="text-lg font-semibold text-slate-900">Reabertura do mes</h2>
        <p className="mt-1 text-sm text-slate-600">
          Reabre o fechamento atual do mes selecionado e retorna as contas do fechamento para pendente.
        </p>
        <button
          type="button"
          onClick={submitReopen}
          disabled={isReopening}
          className="mt-4 rounded-md bg-amber-600 px-4 py-2 text-white disabled:opacity-60"
        >
          {isReopening ? "Reabrindo..." : "Reabrir mes"}
        </button>
      </section>
    </main>
  );
}
