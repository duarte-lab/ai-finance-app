import "next-auth";
import "next-auth/jwt";

declare module "next-auth" {
  interface User {
    backendToken?: string;
    backendRefreshToken?: string;
    tenantId?: string;
    userId?: string;
  }

  interface Account {
    backendToken?: string;
    backendRefreshToken?: string;
    tenantId?: string;
    userId?: string;
    userEmail?: string;
    userName?: string;
  }

  interface Session {
    backendToken: string;
    tenantId: string;
    userId: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    backendToken?: string;
    backendRefreshToken?: string;
    backendTokenExpiresAt?: number | null;
    tenantId?: string;
    userId?: string;
    userEmail?: string;
    userName?: string;
  }
}
