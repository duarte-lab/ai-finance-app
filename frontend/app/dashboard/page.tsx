import { DashboardSummaryView } from "@/components/DashboardSummaryView";
import { redirectToSignInIfUnauthorized, requireBackendToken } from "@/lib/session";
import { getDashboardSummary, getDueNotifications } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const token = await requireBackendToken();
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;

  try {
    const [summary, notifications] = await Promise.all([
      getDashboardSummary({ year, month }, token),
      getDueNotifications(token),
    ]);

    return (
      <DashboardSummaryView
        initialSummary={summary}
        initialNotifications={notifications}
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
