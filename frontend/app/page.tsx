import Link from "next/link";
import { homeFeatureMenuItems } from "@/lib/menu";

export default function Home() {
  return (
    <main className="relative isolate min-h-screen overflow-hidden">
      <div className="absolute inset-0 -z-10 bg-[radial-gradient(circle_at_10%_10%,#dbeafe,transparent_30%),radial-gradient(circle_at_90%_20%,#fee2e2,transparent_35%),linear-gradient(180deg,#f8fafc_0%,#eef2ff_100%)]" />

      <section className="mx-auto flex w-full max-w-6xl flex-col gap-8 px-4 py-14 md:py-20">
        <div className="max-w-3xl rounded-2xl border border-slate-200/70 bg-white/80 p-6 backdrop-blur md:p-8">
          <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">
            Controle financeiro domestico
          </p>
          <h1 className="text-3xl font-semibold leading-tight text-slate-900 md:text-4xl">
            Organize contas da casa, participantes e fechamento mensal em um unico lugar
          </h1>
          <p className="mt-3 text-slate-600">
            Use os atalhos abaixo para acompanhar o dashboard, gerenciar contas, manter pessoas cadastradas e concluir o fechamento do mes com divisao por participante.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {homeFeatureMenuItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="group rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-md"
            >
              <div className="mb-3 flex items-center justify-between">
                <span className="text-sm font-semibold text-slate-900">{item.title}</span>
                {item.badge && (
                  <span className="rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700">
                    {item.badge}
                  </span>
                )}
              </div>
              <p className="text-sm text-slate-600">{item.description}</p>
              <span className="mt-4 inline-block text-sm font-medium text-slate-800 group-hover:text-slate-950">
                Acessar
              </span>
            </Link>
          ))}
        </div>
      </section>
    </main>
  );
}