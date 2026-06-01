"use client";

import { signOut } from "next-auth/react";
import { isUnauthorizedApiError } from "@/services/api";

export function handleApiError(error: unknown, fallbackMessage: string): string | null {
  if (isUnauthorizedApiError(error)) {
    void signOut({ callbackUrl: "/auth/signin" });
    return null;
  }

  return error instanceof Error ? error.message : fallbackMessage;
}