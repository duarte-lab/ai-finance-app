import { DashboardSummaryView } from "@/components/DashboardSummaryView";
import { getDashboardSummary } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = now.getUTCMonth() + 1;

  const summary = await getDashboardSummary({ year, month });

  return (
    <DashboardSummaryView
      initialSummary={summary}
      initialYear={year}
      initialMonth={month}
    />
  );
}
