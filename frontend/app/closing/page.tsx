import { MonthlyClosingView } from "@/components/MonthlyClosingView";
import { getAccounts } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function ClosingPage() {
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;
  const accounts = await getAccounts({ year, month });

  return (
    <MonthlyClosingView
      initialAccounts={accounts}
      initialYear={year}
      initialMonth={month}
    />
  );
}
