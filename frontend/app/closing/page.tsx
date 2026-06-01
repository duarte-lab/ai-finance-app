import { MonthlyClosingView } from "@/components/MonthlyClosingView";
import { redirectToSignInIfUnauthorized, requireBackendToken } from "@/lib/session";
import { getAccounts, getMonthlyClosing, getPeople } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function ClosingPage() {
  const token = await requireBackendToken();
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;

  try {
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
  } catch (error) {
    redirectToSignInIfUnauthorized(error);
  }

  return null;
}
