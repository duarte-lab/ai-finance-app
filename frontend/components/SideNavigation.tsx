"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { drawerMenuSections } from "@/lib/menu";

type SideNavigationProps = {
  isOpen: boolean;
  onClose: () => void;
  onHoverStart?: () => void;
  onHoverEnd?: () => void;
};

function isActivePath(currentPath: string, href?: string) {
  if (!href) {
    return false;
  }

  return href === "/"
    ? currentPath === "/"
    : currentPath === href || currentPath.startsWith(`${href}/`);
}

function SectionIcon({ icon }: { icon: "overview" | "finance" }) {
  if (icon === "overview") {
    return (
      <svg aria-hidden="true" viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="1.8">
        <path d="M4 19h16" />
        <path d="M6 16v-5" />
        <path d="M12 16V8" />
        <path d="M18 16V5" />
      </svg>
    );
  }

  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="1.8">
      <path d="M4 7h16" />
      <path d="M4 12h16" />
      <path d="M4 17h10" />
      <circle cx="18" cy="17" r="2" />
    </svg>
  );
}

function ItemIcon({ icon }: { icon?: "chart" | "wallet" | "calendar" | "users" }) {
  switch (icon) {
    case "chart":
      return (
        <svg aria-hidden="true" viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M3 3v18h18" />
          <path d="M18 17V9M13 17v-4M8 17v-2" />
        </svg>
      );
    case "wallet":
      return (
        <svg aria-hidden="true" viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M21 12a1 1 0 0 0-1-1H7l2-7H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a1 1 0 0 0 1-1z" />
          <circle cx="17" cy="14" r="1" />
        </svg>
      );
    case "calendar":
      return (
        <svg aria-hidden="true" viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="4" width="18" height="18" rx="2" />
          <path d="M16 2v4M8 2v4M3 10h18" />
        </svg>
      );
    case "users":
      return (
        <svg aria-hidden="true" viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
          <circle cx="9" cy="7" r="4" />
          <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
          <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
      );
    default:
      return null;
  }
}

export function SideNavigation({ isOpen, onClose, onHoverStart, onHoverEnd }: SideNavigationProps) {
  const pathname = usePathname();

  return (
    <aside
      aria-label="Menu lateral"
      onMouseEnter={onHoverStart}
      onMouseLeave={onHoverEnd}
      className={`fixed left-0 top-0 z-40 h-full w-72 transform border-r border-slate-200 bg-white/95 pt-20 shadow-xl backdrop-blur transition-transform duration-300 ease-in-out ${
        isOpen ? "translate-x-20" : "-translate-x-full"
      }`}
    >
      <nav className="h-full overflow-y-auto px-6 pb-6">
        <ul className="space-y-5">
          {drawerMenuSections.map((section) => {
            const sectionActive =
              isActivePath(pathname, section.href) ||
              section.items.some((item) => isActivePath(pathname, item.href));

            return (
              <li key={section.title}>
                <div className="mb-2">
                  {section.href ? (
                    <Link
                      href={section.href}
                      onClick={onClose}
                      className={`flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold transition ${
                        sectionActive
                          ? "bg-slate-900 text-white"
                          : "text-slate-800 hover:bg-slate-100"
                      }`}
                    >
                      <SectionIcon icon={section.icon} />
                      <span>{section.title}</span>
                    </Link>
                  ) : (
                    <h2 className="flex items-center gap-2 px-3 text-sm font-semibold text-slate-900">
                      <SectionIcon icon={section.icon} />
                      <span>{section.title}</span>
                    </h2>
                  )}

                  <p className="px-3 pt-1 text-xs text-slate-500">{section.description}</p>
                </div>

                <ul className="space-y-1 ml-4">
                  {section.items.map((item) => {
                    const activeItem = isActivePath(pathname, item.href);

                    return (
                      <li key={item.href}>
                        <Link
                          href={item.href}
                          onClick={onClose}
                          className={`flex items-center gap-2 rounded-lg px-4 py-2 text-sm transition ${
                            activeItem
                              ? "bg-slate-100 font-medium text-slate-900"
                              : "text-slate-700 hover:bg-slate-100 hover:text-slate-900"
                          }`}
                        >
                          {item.icon && <ItemIcon icon={item.icon} />}
                          <span>{item.title}</span>
                        </Link>
                      </li>
                    );
                  })}
                </ul>
              </li>
            );
          })}
        </ul>
      </nav>
    </aside>
  );
}
