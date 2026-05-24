"use client";

import { useEffect, useState } from "react";
import {
  Account,
  AccountParticipant,
  Person,
  createPerson,
  createAccount,
  getAccounts,
  getPeople,
  markAccountAsPaid,
} from "@/services/api";

interface AccountsListProps {
  initialAccounts: Account[];
  initialYear: number;
  initialMonth: number;
}

export function AccountsList({
  initialAccounts,
  initialYear,
  initialMonth,
}: AccountsListProps) {
  const [accounts, setAccounts] = useState<Account[]>(initialAccounts);
  const [people, setPeople] = useState<Person[]>([]);
  const [year, setYear] = useState<number>(initialYear);
  const [month, setMonth] = useState<number>(initialMonth);
  const [isLoading, setIsLoading] = useState(false);
  const [payingId, setPayingId] = useState<string | null>(null);
  const [isPeopleLoading, setIsPeopleLoading] = useState(false);
  const [isCreatingPerson, setIsCreatingPerson] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [amount, setAmount] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [personName, setPersonName] = useState("");
  const [participants, setParticipants] = useState<AccountParticipant[]>([]);

  useEffect(() => {
    void loadPeople();
  }, []);

  async function loadPeople() {
    setIsPeopleLoading(true);

    try {
      const list = await getPeople();
      setPeople(list);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load people.");
    } finally {
      setIsPeopleLoading(false);
    }
  }

  function rebalancePercentages(personIds: string[]): AccountParticipant[] {
    if (personIds.length === 0) {
      return [];
    }

    const base = Number((100 / personIds.length).toFixed(2));
    let consumed = 0;

    return personIds.map((personId, index) => {
      if (index < personIds.length - 1) {
        consumed += base;
        return { personId, percentage: base };
      }

      return { personId, percentage: Number((100 - consumed).toFixed(2)) };
    });
  }

  function toggleParticipant(personId: string) {
    const selectedIds = participants.map((item) => item.personId);
    const nextIds = selectedIds.includes(personId)
      ? selectedIds.filter((item) => item !== personId)
      : [...selectedIds, personId];

    setParticipants(rebalancePercentages(nextIds));
  }

  function updateParticipantPercentage(personId: string, percentage: number) {
    setParticipants((current) =>
      current.map((item) => (item.personId === personId ? { ...item, percentage } : item)),
    );
  }

  async function submitNewPerson(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    try {
      setIsCreatingPerson(true);
      const created = await createPerson(personName);
      setPeople((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
      setPersonName("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create person.");
    } finally {
      setIsCreatingPerson(false);
    }
  }

  async function applyFilter() {
    setIsLoading(true);
    setError(null);

    try {
      const filtered = await getAccounts({ year, month });
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
      const updated = await markAccountAsPaid(id);
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

    const percentageTotal = participants.reduce((total, item) => total + item.percentage, 0);
    if (participants.length > 0 && Math.abs(percentageTotal - 100) > 0.01) {
      setError("A soma dos percentuais dos participantes deve ser 100%.");
      return;
    }

    try {
      const created = await createAccount({
        name,
        amount: Number(amount),
        dueDate: new Date(`${dueDate}T00:00:00.000Z`).toISOString(),
        participants,
      });

      setAccounts((prev) => [created, ...prev]);
      setName("");
      setAmount("");
      setDueDate("");
      setParticipants([]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create account.");
    }
  }

  function getPersonName(personId: string): string {
    return people.find((item) => item.id === personId)?.name ?? personId;
  }

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-6">
      <header className="flex flex-col gap-3 rounded-xl border border-slate-200 p-4">
        <h1 className="text-2xl font-semibold text-slate-900">Contas</h1>
        <form id="nova-pessoa" onSubmit={submitNewPerson} className="flex flex-wrap items-end gap-3 rounded-lg border border-slate-200 p-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Nova pessoa
            <input
              aria-label="Nome da pessoa"
              value={personName}
              onChange={(e) => setPersonName(e.target.value)}
              placeholder="Ex.: Ana"
              required
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <button
            type="submit"
            disabled={isCreatingPerson}
            className="rounded-md bg-indigo-700 px-4 py-2 text-white disabled:opacity-60"
          >
            {isCreatingPerson ? "Salvando..." : "Cadastrar pessoa"}
          </button>
          <p className="text-sm text-slate-600">
            {isPeopleLoading ? "Carregando pessoas..." : `${people.length} pessoa(s) cadastrada(s)`}
          </p>
        </form>

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

          <div className="md:col-span-4 rounded-md border border-slate-200 p-3">
            <h2 className="text-sm font-semibold text-slate-900">Despesa compartilhada</h2>
            <p className="mb-2 text-xs text-slate-600">
              Selecione participantes e ajuste percentuais (a soma deve ser 100%).
            </p>

            {people.length === 0 ? (
              <p className="text-sm text-slate-600">Cadastre pessoas para compartilhar despesas.</p>
            ) : (
              <div className="space-y-2">
                {people.map((person) => {
                  const selected = participants.find((item) => item.personId === person.id);

                  return (
                    <div key={person.id} className="flex flex-wrap items-center gap-3">
                      <label className="flex items-center gap-2 text-sm text-slate-700">
                        <input
                          aria-label={`Selecionar participante ${person.name}`}
                          type="checkbox"
                          checked={Boolean(selected)}
                          onChange={() => toggleParticipant(person.id)}
                        />
                        {person.name}
                      </label>

                      {selected && (
                        <label className="flex items-center gap-2 text-sm text-slate-700">
                          %
                          <input
                            aria-label={`Percentual de ${person.name}`}
                            type="number"
                            min={0.01}
                            max={100}
                            step={0.01}
                            value={selected.percentage}
                            onChange={(event) =>
                              updateParticipantPercentage(person.id, Number(event.target.value))
                            }
                            className="w-24 rounded-md border border-slate-300 px-2 py-1"
                          />
                        </label>
                      )}
                    </div>
                  );
                })}

                <p className="text-xs text-slate-600">
                  Soma atual: {participants.reduce((total, item) => total + item.percentage, 0).toFixed(2)}%
                </p>
              </div>
            )}
          </div>
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
                {account.participants.length > 0 && (
                  <span className="text-sm text-slate-600">
                    Compartilhado: {account.participants.map((item) => `${getPersonName(item.personId)} (${item.percentage}%)`).join(", ")}
                  </span>
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
              </div>
            </article>
          ))
        )}
      </section>
    </main>
  );
}
