"use client";

import { useState } from "react";
import { createPerson, deletePerson, Person } from "@/services/api";
import { handleApiError } from "@/lib/client-auth";

interface PeopleManagerProps {
  initialPeople: Person[];
  token?: string;
}

function formatDate(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toISOString().slice(0, 10);
}

function personTypeLabel(type: Person["type"]): string {
  return type === "Owner" ? "Owner" : "Guest";
}

export function PeopleManager({ initialPeople, token }: PeopleManagerProps) {
  const [people, setPeople] = useState<Person[]>(
    [...initialPeople].sort((a, b) => a.name.localeCompare(b.name)),
  );
  const [name, setName] = useState("");
  const [isCreating, setIsCreating] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function handleCreate(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    try {
      setIsCreating(true);
      const created = await createPerson(name, token);
      setPeople((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
      setName("");
    } catch (err) {
      const message = handleApiError(err, "Falha ao criar pessoa.");
      if (message) setError(message);
    } finally {
      setIsCreating(false);
    }
  }

  async function handleDelete(id: string) {
    setError(null);

    try {
      setDeletingId(id);
      await deletePerson(id, token);
      setPeople((prev) => prev.filter((person) => person.id !== id));
    } catch (err) {
      const message = handleApiError(err, "Falha ao excluir pessoa.");
      if (message) setError(message);
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <main className="mx-auto w-full max-w-4xl px-4 py-10">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-slate-900">Pessoas</h1>
        <p className="mt-2 text-sm text-slate-600">
          Gerencie as pessoas que participam do fechamento mensal.
        </p>
      </header>

      <section className="mb-6 rounded-xl border border-slate-200 bg-white p-4">
        <form onSubmit={handleCreate} className="flex flex-wrap items-end gap-3">
          <label className="flex min-w-56 flex-1 flex-col gap-1 text-sm text-slate-700">
            Nome
            <input
              aria-label="Nome da pessoa"
              value={name}
              onChange={(event) => setName(event.target.value)}
              maxLength={50}
              required
              placeholder="Ex.: Ana"
              className="rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <button
            type="submit"
            disabled={isCreating}
            className="rounded-md bg-indigo-700 px-4 py-2 text-white disabled:opacity-60"
          >
            {isCreating ? "Salvando..." : "Adicionar pessoa"}
          </button>
        </form>
      </section>

      {error && <p className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      {people.length === 0 ? (
        <section className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-slate-700">
          Nenhuma pessoa cadastrada.
        </section>
      ) : (
        <section className="rounded-xl border border-slate-200 bg-white">
          <ul>
            {people.map((person) => (
              <li
                key={person.id}
                className="flex items-center justify-between gap-3 border-b border-slate-100 px-4 py-3 last:border-b-0"
              >
                <div className="flex flex-col">
                  <span className="font-medium text-slate-900">{person.name}</span>
                  <span className="text-xs text-slate-500">Tipo: {personTypeLabel(person.type)}</span>
                  <span className="text-xs text-slate-500">
                    Criada em {formatDate(person.createdAtUtc)}
                  </span>
                </div>

                {person.type === "Owner" ? (
                  <span className="rounded-md bg-slate-100 px-3 py-2 text-sm text-slate-600">
                    Não removível
                  </span>
                ) : (
                  <button
                    type="button"
                    onClick={() => handleDelete(person.id)}
                    disabled={deletingId === person.id}
                    className="rounded-md bg-rose-600 px-3 py-2 text-sm text-white disabled:opacity-60"
                  >
                    {deletingId === person.id ? "Excluindo..." : "Excluir"}
                  </button>
                )}
              </li>
            ))}
          </ul>
        </section>
      )}
    </main>
  );
}
