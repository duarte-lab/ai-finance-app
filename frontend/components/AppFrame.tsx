"use client";

import { useState } from "react";
import { MainNavigation } from "@/components/MainNavigation";
import { SideNavigation } from "@/components/SideNavigation";

type AppFrameProps = {
  children: React.ReactNode;
};

export function AppFrame({ children }: AppFrameProps) {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

  return (
    <div className="min-h-full">
      <MainNavigation onToggleDrawer={() => setIsDrawerOpen((open) => !open)} />

      <div className="mx-auto flex w-full max-w-6xl md:gap-6 md:px-4 md:py-6">
        <SideNavigation isOpen={isDrawerOpen} onClose={() => setIsDrawerOpen(false)} />
        <div className="w-full px-4 py-6 md:px-0 md:py-0">{children}</div>
      </div>
    </div>
  );
}
