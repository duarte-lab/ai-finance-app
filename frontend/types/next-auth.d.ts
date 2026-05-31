import "next-auth";
import "next-auth/jwt";

declare module "next-auth" {
  interface Session {
    backendToken: string;
    tenantId: string;
    userId: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    backendToken?: string;
    tenantId?: string;
    userId?: string;
    userEmail?: string;
    userName?: string;
  }
}
