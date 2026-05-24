const serverApiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";
const browserApiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? serverApiBaseUrl;

function getApiBaseUrl(): string {
  return typeof window === "undefined" ? serverApiBaseUrl : browserApiBaseUrl;
}

export interface Account {
  id: string;
  name: string;
  amount: number;
  dueDate: string;
  createdAtUtc: string;
  paid: boolean;
  participatesInDivision: boolean;
  participants: AccountParticipant[];
}

export interface AccountParticipant {
  personId: string;
  percentage: number;
}

export interface Person {
  id: string;
  name: string;
  createdAtUtc: string;
}

export interface CreateAccountRequest {
  name: string;
  amount: number;
  dueDate: string;
  participatesInDivision?: boolean;
  participants?: AccountParticipant[];
}

export interface UpdateAccountRequest {
  name: string;
  amount: number;
  dueDate: string;
  paid: boolean;
  participatesInDivision: boolean;
  participants?: AccountParticipant[];
}

export interface DashboardCategoryPoint {
  label: string;
  amount: number;
  count: number;
}

export interface DashboardSummary {
  year: number;
  month: number;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  totalCount: number;
  paidCount: number;
  pendingCount: number;
  chart: DashboardCategoryPoint[];
}

export interface CreateMonthlyClosingRequest {
  year: number;
  month: number;
  accountIds: string[];
  participants: string[];
}

export interface MonthlyClosingResult {
  id: string;
  year: number;
  month: number;
  totalAmount: number;
  amountPerPerson: number;
  accountCount: number;
  participantCount: number;
  closedAtUtc: string;
}

export type NotificationType = "DueInThreeDays" | "DueToday";

export interface DueNotification {
  accountId: string;
  accountName: string;
  dueDateUtc: string;
  daysUntilDue: number;
  type: NotificationType;
  message: string;
}

interface GetAccountsFilter {
  year?: number;
  month?: number;
}

export async function getAccounts(filter?: GetAccountsFilter): Promise<Account[]> {
  const url = new URL(`${getApiBaseUrl()}/api/accounts`);

  if (filter?.year && filter?.month) {
    url.searchParams.set("year", String(filter.year));
    url.searchParams.set("month", String(filter.month));
  }

  const res = await fetch(url.toString(), { cache: "no-store" });

  if (!res.ok) {
    throw new Error(`Failed to fetch accounts: ${res.status}`);
  }

  return (await res.json()) as Account[];
}

export async function markAccountAsPaid(id: string): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}/pay`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
  });

  if (!res.ok) {
    throw new Error(`Failed to mark account as paid: ${res.status}`);
  }

  return (await res.json()) as Account;
}

export async function createAccount(request: CreateAccountRequest): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    throw new Error(`Failed to create account: ${res.status}`);
  }

  return (await res.json()) as Account;
}

export async function updateAccount(id: string, request: UpdateAccountRequest): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    throw new Error(`Failed to update account: ${res.status}`);
  }

  return (await res.json()) as Account;
}

export async function updateAccountDivisionParticipation(
  id: string,
  participatesInDivision: boolean,
): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}/division-participation`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ participatesInDivision }),
  });

  if (!res.ok) {
    throw new Error(`Failed to update account division participation: ${res.status}`);
  }

  return (await res.json()) as Account;
}

export async function getPeople(): Promise<Person[]> {
  const res = await fetch(`${getApiBaseUrl()}/api/people`, { cache: "no-store" });

  if (!res.ok) {
    throw new Error(`Failed to fetch people: ${res.status}`);
  }

  return (await res.json()) as Person[];
}

export async function createPerson(name: string): Promise<Person> {
  const res = await fetch(`${getApiBaseUrl()}/api/people`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name }),
  });

  if (!res.ok) {
    throw new Error(`Failed to create person: ${res.status}`);
  }

  return (await res.json()) as Person;
}

export async function getDashboardSummary(
  filter?: GetAccountsFilter,
): Promise<DashboardSummary> {
  const url = new URL(`${getApiBaseUrl()}/api/dashboard/summary`);

  if (filter?.year) {
    url.searchParams.set("year", String(filter.year));
  }

  if (filter?.month) {
    url.searchParams.set("month", String(filter.month));
  }

  const res = await fetch(url.toString(), { cache: "no-store" });

  if (!res.ok) {
    throw new Error(`Failed to fetch dashboard summary: ${res.status}`);
  }

  return (await res.json()) as DashboardSummary;
}

export async function createMonthlyClosing(
  request: CreateMonthlyClosingRequest,
): Promise<MonthlyClosingResult> {
  const res = await fetch(`${getApiBaseUrl()}/closing`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    throw new Error(`Failed to create monthly closing: ${res.status}`);
  }

  return (await res.json()) as MonthlyClosingResult;
}

export async function getDueNotifications(): Promise<DueNotification[]> {
  const res = await fetch(`${getApiBaseUrl()}/api/notifications/due`, { cache: "no-store" });

  if (!res.ok) {
    throw new Error(`Failed to fetch due notifications: ${res.status}`);
  }

  return (await res.json()) as DueNotification[];
}