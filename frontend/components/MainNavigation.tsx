import Link from "next/link";
import { headerMenuItems } from "@/lib/menu";

export function MainNavigation() {
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
        </nav>
      </div>
    </header>
  );
}
