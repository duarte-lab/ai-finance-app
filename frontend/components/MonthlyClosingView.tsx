"use client";

import { useMemo, useState } from "react";
import {
  Account,
  MonthlyClosingResult,
  Person,
  createMonthlyClosing,
  getAccounts,
  getMonthlyClosing,
  reopenMonthlyClosing,
} from "@/services/api";

interface MonthlyClosingViewProps {
  initialAccounts: Account[];
  initialPeople: Person[];
  initialClosing: MonthlyClosingResult | null;
  initialYear: number;
  initialMonth: number;
  token?: string;
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

function defaultSelectedAccountIds(accounts: Account[]): string[] {
  return accounts
    .filter((account) => account.participatesInDivision)
    .map((account) => account.id);
}

export function MonthlyClosingView({
  initialAccounts,
  initialPeople,
  initialClosing,
  initialYear,
  initialMonth,
  token,
}: MonthlyClosingViewProps) {
  const [year, setYear] = useState(initialYear);
  const [month, setMonth] = useState(initialMonth);
  const [accounts, setAccounts] = useState<Account[]>(initialAccounts);
  const [people] = useState<Person[]>(initialPeople);
  const [currentClosing, setCurrentClosing] = useState<MonthlyClosingResult | null>(initialClosing);
  const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>(
    defaultSelectedAccountIds(initialAccounts),
  );
  const [selectedParticipants, setSelectedParticipants] = useState<string[]>(
    initialClosing?.participants ?? initialPeople.map((person) => person.name),
  );
  const [isFiltering, setIsFiltering] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReopening, setIsReopening] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

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
    setSuccessMessage(null);

    try {
      const [monthAccounts, monthClosing] = await Promise.all([
        getAccounts({ year, month }, token),
        getMonthlyClosing(year, month, token),
      ]);

      setAccounts(monthAccounts);
      setSelectedAccountIds(defaultSelectedAccountIds(monthAccounts));
      setCurrentClosing(monthClosing);
      setSelectedParticipants(monthClosing?.participants ?? people.map((person) => person.name));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load monthly accounts.");
    } finally {
      setIsFiltering(false);
    }
  }

  function toggleParticipant(name: string) {
    setSelectedParticipants((current) =>
      current.includes(name)
        ? current.filter((item) => item !== name)
        : [...current, name],
    );
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
    setSuccessMessage(null);
    setCurrentClosing(null);

    if (selectedAccountIds.length === 0) {
      setError("Selecione pelo menos uma conta para fechar o mes.");
      return;
    }

    if (selectedParticipants.length === 0) {
      setError("Informe ao menos um participante.");
      return;
    }

    setIsSubmitting(true);

    try {
      const payload = await createMonthlyClosing({
        year,
        month,
        accountIds: selectedAccountIds,
        participants: selectedParticipants,
      }, token);

      setCurrentClosing(payload);
      setSuccessMessage("Mes fechado com sucesso.");
      const refreshed = await getAccounts({ year, month }, token);
      setAccounts(refreshed);
      setSelectedAccountIds(defaultSelectedAccountIds(refreshed));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to close month.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function submitReopen() {
    setIsReopening(true);
    setError(null);
    setSuccessMessage(null);

    try {
      await reopenMonthlyClosing({ year, month }, token);

      const refreshed = await getAccounts({ year, month }, token);
      setAccounts(refreshed);
      setSelectedAccountIds(defaultSelectedAccountIds(refreshed));
      setCurrentClosing(null);
      setSelectedParticipants(people.map((person) => person.name));
      setSuccessMessage("Mes reaberto com sucesso.");
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
          Selecione as contas do mes, carregue participantes da lista de pessoas e divida o total igualmente.
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
      {successMessage && <p className="rounded-md bg-emerald-50 p-3 text-sm text-emerald-700">{successMessage}</p>}

      <form onSubmit={submitClosing} className="rounded-2xl border border-slate-200 bg-white p-6">
        <h2 className="text-lg font-semibold text-slate-900">Contas elegiveis</h2>
        <p className="mt-1 text-sm text-slate-600">
          Contas marcadas para divisao entram automaticamente. Contas nao marcadas podem ser incluidas opcionalmente.
        </p>

        <div className="mt-4 space-y-3">
          {accounts.length === 0 ? (
            <p className="rounded-md border border-dashed border-slate-300 p-4 text-sm text-slate-600">
              Nenhuma conta encontrada para esse mes.
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
                    disabled={currentClosing !== null}
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

                <div className="flex items-center gap-3">
                  <span className="text-sm font-medium text-slate-800">
                    {formatCurrency(account.amount)}
                  </span>
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-medium ${
                      account.paid
                        ? "bg-green-100 text-green-700"
                        : "bg-amber-100 text-amber-700"
                    }`}
                  >
                    {account.paid ? "Pago" : "Pendente"}
                  </span>
                </div>
              </label>
            ))
          )}
        </div>

        <div className="mt-5 rounded-md border border-slate-200 p-4">
          <h3 className="text-sm font-semibold text-slate-900">Participantes</h3>
          <p className="mt-1 text-xs text-slate-600">
            Participantes carregados da lista de pessoas. A divisao sera igual entre os selecionados.
          </p>

          <div className="mt-3 space-y-2">
            {people.length === 0 ? (
              <p className="text-sm text-slate-600">Nenhuma pessoa cadastrada.</p>
            ) : (
              people.map((person) => (
                <label key={person.id} className="flex items-center gap-2 text-sm text-slate-700">
                  <input
                    aria-label={`Selecionar participante ${person.name}`}
                    type="checkbox"
                    checked={selectedParticipants.includes(person.name)}
                    disabled={currentClosing !== null}
                    onChange={() => toggleParticipant(person.name)}
                  />
                  {person.name}
                </label>
              ))
            )}
          </div>
        </div>

        <div className="mt-4 flex items-center justify-between gap-3">
          <p className="text-sm text-slate-600">
            Total selecionado: <strong>{formatCurrency(totalSelected)}</strong>
          </p>

          <button
            type="submit"
            disabled={isSubmitting || currentClosing !== null}
            className="rounded-md bg-blue-700 px-4 py-2 text-white disabled:opacity-60"
          >
            {currentClosing !== null ? "Mes ja fechado" : isSubmitting ? "Fechando..." : "Fechar mes"}
          </button>
        </div>
      </form>

      {currentClosing && (
        <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6">
          <h2 className="text-lg font-semibold text-emerald-900">Resultado do fechamento</h2>
          <p className="mt-2 text-sm text-emerald-800">
            Status: <strong>{currentClosing.isReopened ? "Mes reaberto" : "Mes fechado"}</strong>
          </p>
          <p className="mt-2 text-sm text-emerald-800">
            Total do mes: <strong>{formatCurrency(currentClosing.totalAmount)}</strong>
          </p>
          <p className="text-sm text-emerald-800">
            Valor por pessoa: <strong>{formatCurrency(currentClosing.amountPerPerson)}</strong>
          </p>
          <div className="mt-4 rounded-md bg-white/70 p-4">
            <h3 className="text-sm font-semibold text-emerald-900">Divisao por pessoa</h3>
            <div className="mt-2 space-y-1 text-sm text-emerald-900">
              {(currentClosing.participants ?? []).map((participant) => (
                <p key={participant}>
                  {participant}: <strong>{formatCurrency(currentClosing.amountPerPerson)}</strong>
                </p>
              ))}
            </div>
          </div>
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
          disabled={isReopening || currentClosing === null}
          className="mt-4 rounded-md bg-amber-600 px-4 py-2 text-white disabled:opacity-60"
        >
          {isReopening ? "Reabrindo..." : "Reabrir mes"}
        </button>
      </section>
    </main>
  );
}
