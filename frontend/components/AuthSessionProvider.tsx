"use client";

import { SessionProvider as NextAuthSessionProvider } from "next-auth/react";
import type { Session } from "next-auth";

interface AuthSessionProviderProps {
  children: React.ReactNode;
  session: Session | null;
}

export function AuthSessionProvider({ children, session }: AuthSessionProviderProps) {
  return (
    <NextAuthSessionProvider session={session}>
      {children}
    </NextAuthSessionProvider>
  );
}
