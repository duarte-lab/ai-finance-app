"use client";

import { useEffect, useRef, useState } from "react";
import {
  Account,
  createAccount,
  deleteAccount,
  getAccounts,
  markAccountAsPaid,
  updateAccount,
  updateAccountDivisionParticipation,
} from "@/services/api";
import { handleApiError } from "@/lib/client-auth";
import { MonthNavigation } from "@/components/MonthNavigation";

interface AccountsListProps {
  initialAccounts: Account[];
  initialYear: number;
  initialMonth: number;
  token?: string;
}

function todayUtc(): string {
  return new Date().toISOString().slice(0, 10);
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
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [editingAccountId, setEditingAccountId] = useState<string | null>(null);
  const [openMenuId, setOpenMenuId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [amount, setAmount] = useState("");
  const [dueDate, setDueDate] = useState(todayUtc);
  const [editName, setEditName] = useState("");
  const [editAmount, setEditAmount] = useState("");
  const [editDueDate, setEditDueDate] = useState("");
  const menuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpenMenuId(null);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  async function applyFilter(nextYear = year, nextMonth = month) {
    setIsLoading(true);
    setError(null);

    try {
      const filtered = await getAccounts({ year: nextYear, month: nextMonth }, token);
      setAccounts(filtered);
    } catch (err) {
      const message = handleApiError(err, "Failed to filter accounts.");
      if (message) setError(message);
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
      const message = handleApiError(err, "Failed to pay account.");
      if (message) setError(message);
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
      setDueDate(todayUtc());
    } catch (err) {
      const message = handleApiError(err, "Failed to create account.");
      if (message) setError(message);
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
      const message = handleApiError(err, "Failed to update account.");
      if (message) setError(message);
    }
  }

  async function toggleDivisionParticipation(account: Account) {
    setDivisionUpdatingId(account.id);
    setOpenMenuId(null);
    setError(null);

    try {
      const updated = await updateAccountDivisionParticipation(
        account.id,
        !account.participatesInDivision,
        token,
      );
      setAccounts((prev) => prev.map((item) => (item.id === account.id ? updated : item)));
    } catch (err) {
      const message = handleApiError(err, "Failed to update division participation.");
      if (message) setError(message);
    } finally {
      setDivisionUpdatingId(null);
    }
  }

  async function removeAccount(id: string) {
    setDeletingId(id);
    setOpenMenuId(null);
    setError(null);

    try {
      await deleteAccount(id, token);
      setAccounts((prev) => prev.filter((item) => item.id !== id));
    } catch (err) {
      const message = handleApiError(err, "Failed to delete account.");
      if (message) setError(message);
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-6">
      <section className="w-full rounded-xl border border-slate-200 bg-white p-4">
        <MonthNavigation
          year={year}
          month={month}
          onChange={(nextYear, nextMonth) => {
            setYear(nextYear);
            setMonth(nextMonth);
            void applyFilter(nextYear, nextMonth);
          }}
          ariaLabel="Navegacao mensal de contas"
        />
      </section>

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
      </header>

      {error && <p className="rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <section id="lista-contas" className="flex flex-col gap-3 scroll-mt-24" ref={menuRef}>
        {accounts.length === 0 ? (
          <p className="rounded-xl border border-dashed border-slate-300 p-6 text-slate-600">
            Nenhuma conta encontrada para o filtro selecionado.
          </p>
        ) : (
          accounts.map((account) => (
            <article
              key={account.id}
              className="flex items-start justify-between rounded-xl border border-slate-200 p-4"
            >
              <div className="flex flex-col gap-1">
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
                <span
                  className={`mt-1 w-fit rounded-full px-3 py-1 text-sm ${
                    account.paid ? "bg-green-100 text-green-700" : "bg-amber-100 text-amber-700"
                  }`}
                >
                  {account.paid ? "Pago" : "Pendente"}
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
                    <button
                      type="button"
                      onClick={() => saveEdit(account)}
                      className="col-span-full rounded-md bg-slate-700 px-4 py-2 text-sm text-white"
                    >
                      Salvar
                    </button>
                  </div>
                )}
              </div>

              <div className="relative">
                <button
                  type="button"
                  aria-label={`Acoes da conta ${account.name}`}
                  onClick={() => setOpenMenuId((prev) => (prev === account.id ? null : account.id))}
                  className="rounded-md border border-slate-200 px-3 py-2 text-slate-600 hover:bg-slate-50"
                >
                  ...
                </button>

                {openMenuId === account.id && (
                  <div className="absolute right-0 z-10 mt-1 w-52 rounded-md border border-slate-200 bg-white shadow-lg">
                    <button
                      type="button"
                      onClick={() => { void payAccount(account.id); setOpenMenuId(null); }}
                      disabled={account.paid || payingId === account.id}
                      className="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {payingId === account.id ? "Processando..." : "Pagar"}
                    </button>

                    <button
                      type="button"
                      onClick={() => { startEditing(account); setOpenMenuId(null); }}
                      className="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-50"
                    >
                      Editar
                    </button>

                    <button
                      type="button"
                      onClick={() => toggleDivisionParticipation(account)}
                      disabled={divisionUpdatingId === account.id}
                      className="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {account.participatesInDivision ? "Remover da divisao" : "Marcar na divisao"}
                    </button>

                    <button
                      type="button"
                      onClick={() => void removeAccount(account.id)}
                      disabled={deletingId === account.id}
                      className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {deletingId === account.id ? "Excluindo..." : "Excluir"}
                    </button>
                  </div>
                )}
              </div>
            </article>
          ))
        )}
      </section>
    </main>
  );
}
