"use client";

import Link from "next/link";
import { signOut, useSession } from "next-auth/react";

export function MainNavigation() {
  const { data: session } = useSession();

  return (
    <header className="sticky top-0 z-20 border-b border-slate-200/70 bg-white/90 backdrop-blur">
      <div className="mx-auto flex w-full items-center justify-between px-4 py-3">
        <Link href="/" className="flex items-center gap-2 rounded-md px-1 py-1 text-slate-900">
          <span className="inline-block h-2.5 w-2.5 rounded-full bg-emerald-500" aria-hidden="true" />
          <span className="text-sm font-semibold tracking-wide">AI FINANCE APP</span>
        </Link>

        {session?.user && (
          <div className="ml-2 flex items-center gap-3 border-l border-slate-200 pl-3">
            <span className="hidden text-xs text-slate-500 sm:block">
              {session.user.name ?? session.user.email}
            </span>
            <button
              onClick={() => signOut({ callbackUrl: "/auth/signin" })}
              className="rounded-md px-3 py-2 text-sm text-slate-700 transition hover:bg-slate-100"
            >
              Sair
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
