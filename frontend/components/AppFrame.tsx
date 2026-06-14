"use client";

import { useState, useRef, useEffect } from "react";
import { MainNavigation } from "@/components/MainNavigation";
import { IconBar } from "@/components/IconBar";
import { SideNavigation } from "@/components/SideNavigation";

type AppFrameProps = {
  children: React.ReactNode;
};

export function AppFrame({ children }: AppFrameProps) {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [isPinned, setIsPinned] = useState(false);
  const [isHoveringIcons, setIsHoveringIcons] = useState(false);
  const [isHoveringDrawer, setIsHoveringDrawer] = useState(false);
  const closeTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const shouldDrawerBeOpen = isDrawerOpen || isPinned || isHoveringIcons || isHoveringDrawer;
  const contentOffsetClass = shouldDrawerBeOpen ? "ml-[23rem]" : "ml-20";

  const clearCloseTimeout = () => {
    if (closeTimeoutRef.current) {
      clearTimeout(closeTimeoutRef.current);
      closeTimeoutRef.current = null;
    }
  };

  const handleDrawerClose = () => {
    if (!isPinned) {
      clearCloseTimeout();
      setIsDrawerOpen(false);
    }
  };

  const handleIconHover = () => {
    clearCloseTimeout();
    setIsHoveringIcons(true);
    setIsDrawerOpen(true);
  };

  const handleIconLeave = () => {
    setIsHoveringIcons(false);

    if (!isPinned) {
      clearCloseTimeout();
      closeTimeoutRef.current = setTimeout(() => {
        if (!isHoveringDrawer) {
          setIsDrawerOpen(false);
        }
      }, 140);
    }
  };

  const handleDrawerHover = () => {
    clearCloseTimeout();
    setIsHoveringDrawer(true);
    setIsDrawerOpen(true);
  };

  const handleDrawerLeave = () => {
    setIsHoveringDrawer(false);

    if (!isPinned && !isHoveringIcons) {
      clearCloseTimeout();
      closeTimeoutRef.current = setTimeout(() => {
        setIsDrawerOpen(false);
      }, 140);
    }
  };

  const handleTogglePin = () => {
    setIsPinned((prev) => !prev);
    if (!isPinned) {
      setIsDrawerOpen(true);
    }
  };

  useEffect(() => {
    return () => {
      clearCloseTimeout();
    };
  }, []);

  return (
    <div className="min-h-full">
      <div className={`transition-[margin-left] duration-300 ease-in-out ${contentOffsetClass}`}>
        <MainNavigation />
      </div>
      
      <div className="flex">
        <IconBar
          onIconHover={handleIconHover}
          onIconLeave={handleIconLeave}
          isPinned={isPinned}
          onTogglePin={handleTogglePin}
        />
        
        <SideNavigation
          isOpen={shouldDrawerBeOpen}
          onClose={handleDrawerClose}
          onHoverStart={handleDrawerHover}
          onHoverEnd={handleDrawerLeave}
        />
        
        <main className={`flex-1 transition-[margin-left] duration-300 ease-in-out ${contentOffsetClass}`}>
          {children}
        </main>
      </div>
    </div>
  );
}
