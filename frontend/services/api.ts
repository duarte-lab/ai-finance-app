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
  paid: boolean;
}

export interface CreateAccountRequest {
  name: string;
  amount: number;
  dueDate: string;
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