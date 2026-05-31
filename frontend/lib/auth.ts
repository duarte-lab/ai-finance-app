import type { NextAuthOptions } from "next-auth";
import GoogleProvider from "next-auth/providers/google";

const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";

export const authOptions: NextAuthOptions = {
  providers: [
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

          const data = await res.json();
          account.backendToken = data.accessToken;
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
    async jwt({ token, account }) {
      if (account?.backendToken) {
        token.backendToken = account.backendToken as string;
        token.tenantId = account.tenantId as string;
        token.userId = account.userId as string;
        token.userEmail = account.userEmail as string;
        token.userName = account.userName as string;
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
