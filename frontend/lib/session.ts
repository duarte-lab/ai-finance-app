import { getServerSession } from "next-auth";
import { redirect } from "next/navigation";
import { authOptions } from "@/lib/auth";
import { isUnauthorizedApiError } from "@/services/api";

export async function getBackendToken(): Promise<string | undefined> {
  const session = await getServerSession(authOptions);
  return session?.backendToken;
}

export async function requireBackendToken(): Promise<string> {
  const token = await getBackendToken();

  if (!token) {
    redirect("/auth/signin");
  }

  return token;
}

export function redirectToSignInIfUnauthorized(error: unknown): never {
  if (isUnauthorizedApiError(error)) {
    redirect("/auth/signin");
  }

  throw error;
}

export function authHeaders(token?: string): HeadersInit {
  if (!token) return {};
  return { Authorization: `Bearer ${token}` };
}
