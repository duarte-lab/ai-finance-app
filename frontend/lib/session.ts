import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function getBackendToken(): Promise<string | undefined> {
  const session = await getServerSession(authOptions);
  return session?.backendToken;
}

export function authHeaders(token?: string): HeadersInit {
  if (!token) return {};
  return { Authorization: `Bearer ${token}` };
}
