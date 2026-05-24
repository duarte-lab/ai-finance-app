import { DashboardSummaryView } from "@/components/DashboardSummaryView";
import { getDashboardSummary, getDueNotifications } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;

  const [summary, notifications] = await Promise.all([
    getDashboardSummary({ year, month }),
    getDueNotifications(),
  ]);

  return (
    <DashboardSummaryView
      initialSummary={summary}
      initialNotifications={notifications}
      initialYear={year}
      initialMonth={month}
    />
  );
}
