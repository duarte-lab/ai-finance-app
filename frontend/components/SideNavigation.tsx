"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { drawerMenuSections } from "@/lib/menu";

type SideNavigationProps = {
  isOpen: boolean;
  onClose: () => void;
};

function isActivePath(currentPath: string, href?: string) {
  if (!href) {
    return false;
  }

  return href === "/"
    ? currentPath === "/"
    : currentPath === href || currentPath.startsWith(`${href}/`);
}

export function SideNavigation({ isOpen, onClose }: SideNavigationProps) {
  const pathname = usePathname();

  return (
    <>
      {isOpen && (
        <button
          aria-label="Fechar menu lateral"
          onClick={onClose}
          className="fixed inset-0 z-30 bg-slate-900/35 md:hidden"
          type="button"
        />
      )}

      <aside
        aria-label="Menu lateral"
        className={`fixed left-0 top-0 z-40 h-full w-72 transform border-r border-slate-200 bg-white/95 pt-20 shadow-xl backdrop-blur transition-transform duration-200 md:sticky md:top-20 md:h-[calc(100vh-5rem)] md:translate-x-0 md:rounded-2xl md:border md:pt-4 md:shadow-none ${
          isOpen ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <nav className="h-full overflow-y-auto px-4 pb-6">
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
                        className={`block rounded-lg px-3 py-2 text-sm font-semibold transition ${
                          sectionActive
                            ? "bg-slate-900 text-white"
                            : "text-slate-800 hover:bg-slate-100"
                        }`}
                      >
                        {section.title}
                      </Link>
                    ) : (
                      <h2 className="px-3 text-sm font-semibold text-slate-900">{section.title}</h2>
                    )}

                    <p className="px-3 pt-1 text-xs text-slate-500">{section.description}</p>
                  </div>

                  <ul className="space-y-1">
                    {section.items.map((item) => {
                      const activeItem = isActivePath(pathname, item.href);

                      return (
                        <li key={item.href}>
                          <Link
                            href={item.href}
                            onClick={onClose}
                            className={`block rounded-lg px-3 py-2 text-sm transition ${
                              activeItem
                                ? "bg-slate-100 font-medium text-slate-900"
                                : "text-slate-700 hover:bg-slate-100 hover:text-slate-900"
                            }`}
                          >
                            {item.title}
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
    </>
  );
}
