import { MonthlyClosingView } from "@/components/MonthlyClosingView";
import { getBackendToken } from "@/lib/session";
import { getAccounts, getMonthlyClosing, getPeople } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function ClosingPage() {
  const token = await getBackendToken();
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;
  const [accounts, people, closing] = await Promise.all([
    getAccounts({ year, month }, token),
    getPeople(token),
    getMonthlyClosing(year, month, token),
  ]);

  return (
    <MonthlyClosingView
      initialAccounts={accounts}
      initialPeople={people}
      initialClosing={closing}
      initialYear={year}
      initialMonth={month}
      token={token}
    />
  );
}
