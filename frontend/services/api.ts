const serverApiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";
const browserApiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? serverApiBaseUrl;

export class UnauthorizedApiError extends Error {
  constructor(message = "Session expired. Please sign in again.") {
    super(message);
    this.name = "UnauthorizedApiError";
  }
}

export function isUnauthorizedApiError(error: unknown): error is UnauthorizedApiError {
  return error instanceof UnauthorizedApiError;
}

function getApiBaseUrl(): string {
  return typeof window === "undefined" ? serverApiBaseUrl : browserApiBaseUrl;
}

function buildHeaders(token?: string, extra?: Record<string, string>): HeadersInit {
  const headers: Record<string, string> = { ...extra };
  if (token) headers["Authorization"] = `Bearer ${token}`;
  return headers;
}

function ensureApiSuccess(response: Response, operation: string): void {
  if (response.status === 401) {
    throw new UnauthorizedApiError();
  }

  if (!response.ok) {
    throw new Error(`${operation}: ${response.status}`);
  }
}

export interface Account {
  id: string;
  name: string;
  amount: number;
  dueDate: string;
  createdAtUtc: string;
  paid: boolean;
  participatesInDivision: boolean;
}

export interface Person {
  id: string;
  name: string;
  createdAtUtc: string;
  deletedAtUtc?: string | null;
}

export interface CreateAccountRequest {
  name: string;
  amount: number;
  dueDate: string;
  participatesInDivision?: boolean;
}

export interface UpdateAccountRequest {
  name: string;
  amount: number;
  dueDate: string;
  paid: boolean;
  participatesInDivision: boolean;
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

export interface ReopenMonthlyClosingRequest {
  year: number;
  month: number;
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
  isReopened: boolean;
  reopenedAtUtc?: string | null;
  participants?: string[];
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

export async function getAccounts(filter?: GetAccountsFilter, token?: string): Promise<Account[]> {
  const url = new URL(`${getApiBaseUrl()}/api/accounts`);

  if (filter?.year && filter?.month) {
    url.searchParams.set("year", String(filter.year));
    url.searchParams.set("month", String(filter.month));
  }

  const res = await fetch(url.toString(), {
    cache: "no-store",
    headers: buildHeaders(token),
  });

  ensureApiSuccess(res, "Failed to fetch accounts");

  return (await res.json()) as Account[];
}

export async function markAccountAsPaid(id: string, token?: string): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}/pay`, {
    method: "PATCH",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
  });

  ensureApiSuccess(res, "Failed to mark account as paid");

  return (await res.json()) as Account;
}

export async function createAccount(request: CreateAccountRequest, token?: string): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts`, {
    method: "POST",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(request),
  });

  ensureApiSuccess(res, "Failed to create account");

  return (await res.json()) as Account;
}

export async function updateAccount(id: string, request: UpdateAccountRequest, token?: string): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}`, {
    method: "PUT",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(request),
  });

  ensureApiSuccess(res, "Failed to update account");

  return (await res.json()) as Account;
}

export async function updateAccountDivisionParticipation(
  id: string,
  participatesInDivision: boolean,
  token?: string,
): Promise<Account> {
  const res = await fetch(`${getApiBaseUrl()}/api/accounts/${id}/division-participation`, {
    method: "PATCH",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify({ participatesInDivision }),
  });

  ensureApiSuccess(res, "Failed to update account division participation");

  return (await res.json()) as Account;
}

export async function getPeople(token?: string): Promise<Person[]> {
  const res = await fetch(`${getApiBaseUrl()}/api/people`, {
    cache: "no-store",
    headers: buildHeaders(token),
  });

  ensureApiSuccess(res, "Failed to fetch people");

  return (await res.json()) as Person[];
}

export async function createPerson(name: string, token?: string): Promise<Person> {
  const res = await fetch(`${getApiBaseUrl()}/api/people`, {
    method: "POST",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify({ name }),
  });

  ensureApiSuccess(res, "Failed to create person");

  return (await res.json()) as Person;
}

export async function deletePerson(id: string, token?: string): Promise<void> {
  const res = await fetch(`${getApiBaseUrl()}/api/people/${id}`, {
    method: "DELETE",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
  });

  ensureApiSuccess(res, "Failed to delete person");
}

export async function getDashboardSummary(
  filter?: GetAccountsFilter,
  token?: string,
): Promise<DashboardSummary> {
  const url = new URL(`${getApiBaseUrl()}/api/dashboard/summary`);

  if (filter?.year) {
    url.searchParams.set("year", String(filter.year));
  }

  if (filter?.month) {
    url.searchParams.set("month", String(filter.month));
  }

  const res = await fetch(url.toString(), {
    cache: "no-store",
    headers: buildHeaders(token),
  });

  ensureApiSuccess(res, "Failed to fetch dashboard summary");

  return (await res.json()) as DashboardSummary;
}

export async function createMonthlyClosing(
  request: CreateMonthlyClosingRequest,
  token?: string,
): Promise<MonthlyClosingResult> {
  const res = await fetch(`${getApiBaseUrl()}/closing`, {
    method: "POST",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(request),
  });

  ensureApiSuccess(res, "Failed to create monthly closing");

  return (await res.json()) as MonthlyClosingResult;
}

export async function getMonthlyClosing(
  year: number,
  month: number,
  token?: string,
): Promise<MonthlyClosingResult | null> {
  const url = new URL(`${getApiBaseUrl()}/closing`);
  url.searchParams.set("year", String(year));
  url.searchParams.set("month", String(month));

  const res = await fetch(url.toString(), {
    cache: "no-store",
    headers: buildHeaders(token),
  });

  if (res.status === 404) {
    return null;
  }

  ensureApiSuccess(res, "Failed to fetch monthly closing");

  return (await res.json()) as MonthlyClosingResult;
}

export async function reopenMonthlyClosing(
  request: ReopenMonthlyClosingRequest,
  token?: string,
): Promise<MonthlyClosingResult> {
  const res = await fetch(`${getApiBaseUrl()}/closing/reopen`, {
    method: "POST",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(request),
  });

  ensureApiSuccess(res, "Failed to reopen monthly closing");

  return (await res.json()) as MonthlyClosingResult;
}

export async function getDueNotifications(token?: string): Promise<DueNotification[]> {
  const res = await fetch(`${getApiBaseUrl()}/api/notifications/due`, {
    cache: "no-store",
    headers: buildHeaders(token),
  });

  ensureApiSuccess(res, "Failed to fetch due notifications");

  return (await res.json()) as DueNotification[];
}