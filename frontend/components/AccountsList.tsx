"use client";

import { useState } from "react";
import {
  Account,
  createAccount,
  getAccounts,
  markAccountAsPaid,
  updateAccount,
  updateAccountDivisionParticipation,
} from "@/services/api";

interface AccountsListProps {
  initialAccounts: Account[];
  initialYear: number;
  initialMonth: number;
  token?: string;
}

export function AccountsList({
  initialAccounts,
  initialYear,
  initialMonth,
  token,
}: AccountsListProps) {
  const [accounts, setAccounts] = useState<Account[]>(initialAccounts);
  const [year, setYear] = useState<number>(initialYear);
  const [month, setMonth] = useState<number>(initialMonth);
  const [isLoading, setIsLoading] = useState(false);
  const [payingId, setPayingId] = useState<string | null>(null);
  const [divisionUpdatingId, setDivisionUpdatingId] = useState<string | null>(null);
  const [editingAccountId, setEditingAccountId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [amount, setAmount] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [editName, setEditName] = useState("");
  const [editAmount, setEditAmount] = useState("");
  const [editDueDate, setEditDueDate] = useState("");

  async function applyFilter() {
    setIsLoading(true);
    setError(null);

    try {
      const filtered = await getAccounts({ year, month }, token);
      setAccounts(filtered);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to filter accounts.");
    } finally {
      setIsLoading(false);
    }
  }

  async function payAccount(id: string) {
    setPayingId(id);
    setError(null);

    try {
      const updated = await markAccountAsPaid(id, token);
      setAccounts((prev) => prev.map((item) => (item.id === id ? updated : item)));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to pay account.");
    } finally {
      setPayingId(null);
    }
  }

  async function submitNewAccount(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);

    try {
      const created = await createAccount({
        name,
        amount: Number(amount),
        dueDate: new Date(`${dueDate}T00:00:00.000Z`).toISOString(),
        participatesInDivision: false,
      }, token);

      setAccounts((prev) => [created, ...prev]);
      setName("");
      setAmount("");
      setDueDate("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create account.");
    }
  }

  function startEditing(account: Account) {
    setEditingAccountId(account.id);
    setEditName(account.name);
    setEditAmount(String(account.amount));
    setEditDueDate(new Date(account.dueDate).toISOString().slice(0, 10));
  }

  async function saveEdit(account: Account) {
    setError(null);

    try {
      const updated = await updateAccount(account.id, {
        name: editName,
        amount: Number(editAmount),
        dueDate: new Date(`${editDueDate}T00:00:00.000Z`).toISOString(),
        paid: account.paid,
        participatesInDivision: account.participatesInDivision,
      }, token);

      setAccounts((prev) => prev.map((item) => (item.id === account.id ? updated : item)));
      setEditingAccountId(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update account.");
    }
  }

  async function toggleDivisionParticipation(account: Account) {
    setDivisionUpdatingId(account.id);
    setError(null);

    try {
      const updated = await updateAccountDivisionParticipation(
        account.id,
        !account.participatesInDivision,
        token,
      );
      setAccounts((prev) => prev.map((item) => (item.id === account.id ? updated : item)));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update division participation.");
    } finally {
      setDivisionUpdatingId(null);
    }
  }

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-6">
      <header className="flex flex-col gap-3 rounded-xl border border-slate-200 p-4">
        <h1 className="text-2xl font-semibold text-slate-900">Contas</h1>

        <form id="nova-conta" onSubmit={submitNewAccount} className="grid gap-3 md:grid-cols-4 scroll-mt-24">
          <input
            aria-label="Nome da conta"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Nome"
            required
            className="rounded-md border border-slate-300 px-3 py-2"
          />
          <input
            aria-label="Valor da conta"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder="Valor"
            type="number"
            min="0"
            step="0.01"
            required
            className="rounded-md border border-slate-300 px-3 py-2"
          />
          <input
            aria-label="Data de vencimento"
            value={dueDate}
            onChange={(e) => setDueDate(e.target.value)}
            type="date"
            required
            className="rounded-md border border-slate-300 px-3 py-2"
          />
          <button
            type="submit"
            className="rounded-md bg-blue-700 px-4 py-2 text-white"
          >
            Criar conta
          </button>
        </form>
        <div id="filtro-mensal" className="flex flex-wrap items-end gap-3 scroll-mt-24">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Ano
            <input
              type="number"
              min={1}
              value={year}
              onChange={(e) => setYear(Number(e.target.value))}
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Mês
            <input
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
      </header>

      {error && <p className="rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <section id="lista-contas" className="flex flex-col gap-3 scroll-mt-24">
        {accounts.length === 0 ? (
          <p className="rounded-xl border border-dashed border-slate-300 p-6 text-slate-600">
            Nenhuma conta encontrada para o filtro selecionado.
          </p>
        ) : (
          accounts.map((account) => (
            <article
              key={account.id}
              className="flex items-center justify-between rounded-xl border border-slate-200 p-4"
            >
              <div className="flex flex-col">
                <span className="text-lg font-medium text-slate-900">{account.name}</span>
                <span className="text-sm text-slate-600">
                  Vencimento: {new Date(account.dueDate).toISOString().slice(0, 10)}
                </span>
                <span className="text-sm text-slate-700">R$ {account.amount.toFixed(2)}</span>
                <span className="text-sm text-slate-600">
                  Criada em: {new Date(account.createdAtUtc).toISOString().slice(0, 10)}
                </span>
                <span className="text-sm text-slate-600">
                  Divisao mensal: {account.participatesInDivision ? "Participa" : "Nao participa"}
                </span>

                {editingAccountId === account.id && (
                  <div className="mt-2 grid gap-2 md:grid-cols-3">
                    <input
                      aria-label="Editar nome da conta"
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      className="rounded-md border border-slate-300 px-2 py-1 text-sm"
                    />
                    <input
                      aria-label="Editar valor da conta"
                      type="number"
                      min="0"
                      step="0.01"
                      value={editAmount}
                      onChange={(e) => setEditAmount(e.target.value)}
                      className="rounded-md border border-slate-300 px-2 py-1 text-sm"
                    />
                    <input
                      aria-label="Editar vencimento da conta"
                      type="date"
                      value={editDueDate}
                      onChange={(e) => setEditDueDate(e.target.value)}
                      className="rounded-md border border-slate-300 px-2 py-1 text-sm"
                    />
                  </div>
                )}
              </div>

              <div className="flex items-center gap-3">
                <span
                  className={`rounded-full px-3 py-1 text-sm ${
                    account.paid
                      ? "bg-green-100 text-green-700"
                      : "bg-amber-100 text-amber-700"
                  }`}
                >
                  {account.paid ? "Pago" : "Pendente"}
                </span>

                <button
                  type="button"
                  onClick={() => payAccount(account.id)}
                  disabled={account.paid || payingId === account.id}
                  className="rounded-md bg-emerald-600 px-4 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {payingId === account.id ? "Processando..." : "Pagar"}
                </button>

                <button
                  type="button"
                  onClick={() => toggleDivisionParticipation(account)}
                  disabled={divisionUpdatingId === account.id}
                  className="rounded-md bg-indigo-600 px-4 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {divisionUpdatingId === account.id
                    ? "Atualizando..."
                    : account.participatesInDivision
                      ? "Remover da divisao"
                      : "Marcar na divisao"}
                </button>

                <button
                  type="button"
                  onClick={() => (editingAccountId === account.id ? saveEdit(account) : startEditing(account))}
                  className="rounded-md bg-slate-700 px-4 py-2 text-white"
                >
                  {editingAccountId === account.id ? "Salvar" : "Editar"}
                </button>
              </div>
            </article>
          ))
        )}
      </section>
    </main>
  );
}
