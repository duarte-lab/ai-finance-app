import { AccountsList } from "@/components/AccountsList";
import { getAccounts } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function AccountsPage() {
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;
  const accounts = await getAccounts({ year, month });

  return <AccountsList initialAccounts={accounts} initialYear={year} initialMonth={month} />;
}
