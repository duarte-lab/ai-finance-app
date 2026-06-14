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
      <SideNavigation isOpen={isDrawerOpen} onClose={() => setIsDrawerOpen(false)} />
      <div className="w-full">{children}</div>
    </div>
  );
}
