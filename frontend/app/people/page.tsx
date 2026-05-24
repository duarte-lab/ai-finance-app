import { getPeople } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function PeoplePage() {
  const people = await getPeople();

  return (
    <main className="mx-auto w-full max-w-4xl px-4 py-10">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-slate-900">Pessoas</h1>
        <p className="mt-2 text-sm text-slate-600">
          Pessoas participantes do controle de contas domesticas.
        </p>
      </header>

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
                className="flex items-center justify-between border-b border-slate-100 px-4 py-3 last:border-b-0"
              >
                <span className="font-medium text-slate-900">{person.name}</span>
                <span className="text-xs text-slate-500">{person.createdAtUtc}</span>
              </li>
            ))}
          </ul>
        </section>
      )}
    </main>
  );
}
