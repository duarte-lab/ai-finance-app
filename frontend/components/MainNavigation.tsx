"use client";

import Link from "next/link";
import { signOut, useSession } from "next-auth/react";
import { headerMenuItems } from "@/lib/menu";

export function MainNavigation() {
  const { data: session } = useSession();

  return (
    <header className="sticky top-0 z-20 border-b border-slate-200/70 bg-white/90 backdrop-blur">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3">
        <Link href="/" className="text-sm font-semibold tracking-wide text-slate-900">
          AI FINANCE APP
        </Link>

        <nav aria-label="Menu principal" className="flex items-center gap-2">
          {headerMenuItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="rounded-md px-3 py-2 text-sm text-slate-700 transition hover:bg-slate-100 hover:text-slate-900"
            >
              {item.title}
            </Link>
          ))}

          {session?.user && (
            <div className="ml-2 flex items-center gap-3 border-l border-slate-200 pl-3">
              <span className="text-xs text-slate-500 hidden sm:block">
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
        </nav>
      </div>
    </header>
  );
}
