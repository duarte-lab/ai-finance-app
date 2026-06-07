import type { NextAuthOptions } from "next-auth";
import GoogleProvider from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials";

const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";
const nextAuthUrl = process.env.NEXTAUTH_URL ?? "";
const isHttpsDeployment = /^https:\/\//i.test(nextAuthUrl);
const isSecureCookie = process.env.NODE_ENV === "production" && isHttpsDeployment;

interface BackendAuthResponse {
  accessToken: string;
  refreshToken: string;
  tenantId: string;
  userId: string;
  email: string;
  name: string;
}

function parseJwtExpMs(token: string): number | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const decoded = JSON.parse(Buffer.from(payload, "base64url").toString("utf8")) as { exp?: number };
    return typeof decoded.exp === "number" ? decoded.exp * 1000 : null;
  } catch {
    return null;
  }
}

async function refreshBackendToken(refreshToken: string): Promise<BackendAuthResponse | null> {
  try {
    const res = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) return null;
    return (await res.json()) as BackendAuthResponse;
  } catch {
    return null;
  }
}

export const authOptions: NextAuthOptions = {
  session: {
    strategy: "jwt",
  },
  useSecureCookies: isSecureCookie,
  cookies: {
    sessionToken: {
      name: isSecureCookie ? "__Secure-next-auth.session-token" : "next-auth.session-token",
      options: {
        httpOnly: true,
        sameSite: "lax",
        path: "/",
        secure: isSecureCookie,
      },
    },
  },
  providers: [
    CredentialsProvider({
      name: "Email e Senha",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Senha", type: "password" },
      },
      async authorize(credentials) {
        const email = credentials?.email?.trim();
        const password = credentials?.password;

        if (!email || !password) return null;

        const res = await fetch(`${apiBaseUrl}/api/auth/login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email, password }),
        });

        if (!res.ok) return null;

        const data = (await res.json()) as BackendAuthResponse;
        return {
          id: data.userId,
          email: data.email,
          name: data.name,
          backendToken: data.accessToken,
          backendRefreshToken: data.refreshToken,
          tenantId: data.tenantId,
        };
      },
    }),
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
    }),
  ],
  callbacks: {
    async signIn({ account }) {
      if (account?.provider === "google" && account.id_token) {
        try {
          const res = await fetch(`${apiBaseUrl}/api/auth/google`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idToken: account.id_token }),
          });
          if (!res.ok) return false;

          const data = (await res.json()) as BackendAuthResponse;
          account.backendToken = data.accessToken;
          account.backendRefreshToken = data.refreshToken;
          account.tenantId = data.tenantId;
          account.userId = data.userId;
          account.userEmail = data.email;
          account.userName = data.name;
        } catch {
          return false;
        }
      }
      return true;
    },
    async jwt({ token, account, user }) {
      if (account?.backendToken || user?.backendToken) {
        const backendToken = (account?.backendToken ?? user?.backendToken) as string;
        token.backendToken = backendToken;
        token.backendTokenExpiresAt = parseJwtExpMs(backendToken);
        token.backendRefreshToken =
          (account?.backendRefreshToken ?? user?.backendRefreshToken) as string | undefined;
        token.tenantId = (account?.tenantId ?? user?.tenantId) as string;
        token.userId = (account?.userId ?? user?.id) as string;
        token.userEmail = (account?.userEmail ?? user?.email) as string;
        token.userName = (account?.userName ?? user?.name) as string;
        return token;
      }

      const expiresAt = token.backendTokenExpiresAt;
      if (typeof expiresAt === "number" && Date.now() >= expiresAt - 5000 && token.backendRefreshToken) {
        const refreshed = await refreshBackendToken(token.backendRefreshToken);
        if (!refreshed) {
          delete token.backendToken;
          delete token.backendRefreshToken;
          delete token.backendTokenExpiresAt;
          return token;
        }

        token.backendToken = refreshed.accessToken;
        token.backendRefreshToken = refreshed.refreshToken;
        token.backendTokenExpiresAt = parseJwtExpMs(refreshed.accessToken);
        token.tenantId = refreshed.tenantId;
        token.userId = refreshed.userId;
        token.userEmail = refreshed.email;
        token.userName = refreshed.name;
      }

      return token;
    },
    async session({ session, token }) {
      session.backendToken = token.backendToken as string;
      session.tenantId = token.tenantId as string;
      session.userId = token.userId as string;
      if (session.user) {
        session.user.name = token.userName as string;
        session.user.email = token.userEmail as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/auth/signin",
  },
};
