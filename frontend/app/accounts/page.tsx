import { AccountsList } from "@/components/AccountsList";
import { redirectToSignInIfUnauthorized, requireBackendToken } from "@/lib/session";
import { getAccounts } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function AccountsPage() {
  const token = await requireBackendToken();
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;

  try {
    const accounts = await getAccounts({ year, month }, token);
    return <AccountsList initialAccounts={accounts} initialYear={year} initialMonth={month} token={token} />;
  } catch (error) {
    redirectToSignInIfUnauthorized(error);
  }
}
