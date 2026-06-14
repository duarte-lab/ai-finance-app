"use client";

import { usePathname } from "next/navigation";
import { drawerMenuSections } from "@/lib/menu";

type IconBarProps = {
  onIconHover: () => void;
  onIconLeave: () => void;
  isPinned: boolean;
  onTogglePin: () => void;
};

function SectionIcon({ icon }: { icon: "overview" | "finance" }) {
  if (icon === "overview") {
    return (
      <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8">
        <path d="M4 19h16" />
        <path d="M6 16v-5" />
        <path d="M12 16V8" />
        <path d="M18 16V5" />
      </svg>
    );
  }

  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8">
      <path d="M4 7h16" />
      <path d="M4 12h16" />
      <path d="M4 17h10" />
      <circle cx="18" cy="17" r="2" />
    </svg>
  );
}

function isActivePath(currentPath: string, href?: string) {
  if (!href) {
    return false;
  }

  return href === "/"
    ? currentPath === "/"
    : currentPath === href || currentPath.startsWith(`${href}/`);
}

export function IconBar({ onIconHover, onIconLeave, isPinned, onTogglePin }: IconBarProps) {
  const pathname = usePathname();

  return (
    <aside
      aria-label="Barra de ícones"
      className="fixed left-0 top-0 z-50 h-full w-20 flex flex-col border-r border-slate-200 bg-white shadow-sm"
      onMouseEnter={onIconHover}
      onMouseLeave={onIconLeave}
    >
      <div className="flex-1 flex flex-col items-center gap-4 overflow-y-auto px-2 py-6 pt-20">
        {drawerMenuSections.map((section) => {
          const sectionActive =
            isActivePath(pathname, section.href) ||
            section.items.some((item) => isActivePath(pathname, item.href));

          return (
            <button
              key={section.title}
              type="button"
              onClick={onIconHover}
              title={section.title}
              aria-label={section.title}
              className={`flex h-10 w-10 items-center justify-center rounded-lg transition ${
                sectionActive
                  ? "bg-slate-900 text-white"
                  : "text-slate-600 hover:bg-slate-100 hover:text-slate-900"
              }`}
            >
              <SectionIcon icon={section.icon} />
            </button>
          );
        })}
      </div>

      <div className="border-t border-slate-200 p-2">
        <button
          type="button"
          onClick={onTogglePin}
          title={isPinned ? "Desafixar menu" : "Afixar menu"}
          aria-label={isPinned ? "Desafixar menu" : "Afixar menu"}
          className={`flex items-center justify-center w-full rounded-lg p-2 transition ${
            isPinned
              ? "bg-slate-900 text-white"
              : "text-slate-600 hover:bg-slate-100 hover:text-slate-900"
          }`}
        >
          {isPinned ? (
            <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="currentColor">
              <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V5h14v14z" />
            </svg>
          ) : (
            <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 1 0 4 0M9 5a2 2 0 0 1 4 0m6 6h-6m0 4h6" />
            </svg>
          )}
        </button>
      </div>
    </aside>
  );
}
