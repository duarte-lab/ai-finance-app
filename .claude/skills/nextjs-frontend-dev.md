---
name: nextjs-frontend-dev
description: "Use when developing Next.js 16 (App Router) frontend features, React 19 components, API service calls, Tailwind CSS styling, and unit/component tests. Triggers on: creating pages, components, layouts, Server Components, Client Components, fetch API services, environment variables, Jest tests, React Testing Library, mocking fetch, testing async components."
argument-hint: "Describe the feature or component to implement (e.g. 'create TransactionList page with API call and tests')"
---

# Next.js 16 + React 19 Frontend Development

## Stack
- Next.js 16.2.6 — App Router (Server Components by default)
- React 19.2.4
- TypeScript 5 (strict mode, `@/*` path alias)
- Tailwind CSS v4
- Jest + React Testing Library for unit/component tests

> ⚠️ **IMPORTANT**: Before writing any Next.js code, read `node_modules/next/dist/docs/` for the exact API of this version. Do NOT rely on older Next.js patterns.

---

## Project Structure

```
frontend/
├── app/               # Pages, layouts, routes (App Router)
│   ├── layout.tsx     # Root layout
│   ├── page.tsx       # Home page (Server Component)
│   └── [feature]/
│       ├── page.tsx
│       └── components/   # Feature-scoped components
├── components/        # Shared components
├── services/          # API call functions (fetch-based)
│   └── api.ts
└── __tests__/         # Tests mirror src structure
```

---

## Rules

- **Server Components by default** — only add `"use client"` when you need: browser APIs, event handlers, `useState`, `useEffect`
- **No `any`** — always type API responses with interfaces/types
- **`@/` alias** for all imports (never relative `../../`)
- **UTC** for all date handling
- **DTOs** — define TypeScript interfaces for every API response shape
- **env vars** — always use `process.env.API_BASE_URL` (no hardcoded URLs)

---

## Procedure

### 1. API Service (`services/api.ts`)
- One function per API operation
- Always validate `res.ok` and throw descriptive errors
- Use `cache: "no-store"` for dynamic data, `next: { revalidate: N }` for ISR
- Type the return value

```typescript
// services/api.ts
const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";

export interface Account {
  id: string;
  name: string;
  amount: number;
  dueDate: string;
  paid: boolean;
}

export async function getAccounts(): Promise<Account[]> {
  const res = await fetch(`${apiBaseUrl}/api/accounts`, { cache: "no-store" });

  if (!res.ok) {
    throw new Error(`Failed to fetch accounts: ${res.status}`);
  }

  return res.json();
}

export async function createAccount(data: CreateAccountRequest): Promise<Account> {
  const res = await fetch(`${apiBaseUrl}/api/accounts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    throw new Error(`Failed to create account: ${res.status}`);
  }

  return res.json();
}
```

### 2. Server Component Page
- `async` function, fetch data directly
- Use `export const dynamic = "force-dynamic"` for fully dynamic pages
- Handle errors with error boundaries or try/catch

```typescript
// app/accounts/page.tsx
import { getAccounts, Account } from "@/services/api";

export const dynamic = "force-dynamic";

export default async function AccountsPage() {
  const accounts = await getAccounts();

  return (
    <main className="p-4">
      <h1 className="text-2xl font-bold mb-4">Accounts</h1>
      <AccountList accounts={accounts} />
    </main>
  );
}
```

### 3. Client Component (when needed)
- Add `"use client"` only at the top of the file
- Use for forms, interactive UI, browser events

```typescript
// app/accounts/components/CreateAccountForm.tsx
"use client";

import { useState } from "react";
import { createAccount } from "@/services/api";

interface Props {
  onSuccess: () => void;
}

export function CreateAccountForm({ onSuccess }: Props) {
  const [name, setName] = useState("");
  const [amount, setAmount] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await createAccount({ name, amount: parseFloat(amount) });
    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-2">
      <input
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="Name"
        className="border rounded px-2 py-1"
      />
      <input
        value={amount}
        onChange={(e) => setAmount(e.target.value)}
        type="number"
        placeholder="Amount"
        className="border rounded px-2 py-1"
      />
      <button type="submit" className="bg-blue-600 text-white rounded px-4 py-2">
        Save
      </button>
    </form>
  );
}
```

### 4. Shared Component
```typescript
// components/AccountCard.tsx
import { Account } from "@/services/api";

interface Props {
  account: Account;
}

export function AccountCard({ account }: Props) {
  return (
    <div className="border rounded p-3 flex justify-between items-center">
      <span className="font-medium">{account.name}</span>
      <span className="text-green-600">R$ {account.amount.toFixed(2)}</span>
    </div>
  );
}
```

---

## Unit Tests

### Setup
Install testing dependencies (if not present):
```bash
npm install -D jest jest-environment-jsdom @testing-library/react @testing-library/jest-dom @types/jest ts-jest
```

Add to `package.json`:
```json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch"
  },
  "jest": {
    "testEnvironment": "jsdom",
    "setupFilesAfterFramework": ["<rootDir>/jest.setup.ts"],
    "moduleNameMapper": {
      "^@/(.*)$": "<rootDir>/$1"
    },
    "transform": {
      "^.+\\.tsx?$": "ts-jest"
    }
  }
}
```

`jest.setup.ts`:
```typescript
import "@testing-library/jest-dom";
```

### Testing Service Functions (`__tests__/services/api.test.ts`)
Mock `fetch` globally — never make real HTTP calls in tests:

```typescript
import { getAccounts, createAccount } from "@/services/api";

const mockFetch = jest.fn();
global.fetch = mockFetch;

beforeEach(() => mockFetch.mockClear());

describe("getAccounts", () => {
  it("returns accounts on success", async () => {
    const mockData = [{ id: "1", name: "Rent", amount: 1200, dueDate: "2026-06-01", paid: false }];
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockData,
    });

    const result = await getAccounts();

    expect(result).toEqual(mockData);
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts"),
      expect.objectContaining({ cache: "no-store" })
    );
  });

  it("throws on non-ok response", async () => {
    mockFetch.mockResolvedValueOnce({ ok: false, status: 500 });

    await expect(getAccounts()).rejects.toThrow("Failed to fetch accounts: 500");
  });
});
```

### Testing Client Components (`__tests__/components/AccountCard.test.tsx`)
```typescript
import { render, screen } from "@testing-library/react";
import { AccountCard } from "@/components/AccountCard";

const mockAccount = {
  id: "1",
  name: "Rent",
  amount: 1200,
  dueDate: "2026-06-01",
  paid: false,
};

describe("AccountCard", () => {
  it("renders account name and amount", () => {
    render(<AccountCard account={mockAccount} />);

    expect(screen.getByText("Rent")).toBeInTheDocument();
    expect(screen.getByText("R$ 1200.00")).toBeInTheDocument();
  });
});
```

### Testing Client Components with interaction (`__tests__/components/CreateAccountForm.test.tsx`)
```typescript
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { CreateAccountForm } from "@/app/accounts/components/CreateAccountForm";
import * as api from "@/services/api";

jest.mock("@/services/api");

describe("CreateAccountForm", () => {
  it("calls createAccount and onSuccess on submit", async () => {
    const mockCreate = jest.spyOn(api, "createAccount").mockResolvedValueOnce({
      id: "1", name: "Salary", amount: 3000, dueDate: "2026-06-01", paid: false,
    });
    const onSuccess = jest.fn();

    render(<CreateAccountForm onSuccess={onSuccess} />);

    fireEvent.change(screen.getByPlaceholderText("Name"), { target: { value: "Salary" } });
    fireEvent.change(screen.getByPlaceholderText("Amount"), { target: { value: "3000" } });
    fireEvent.click(screen.getByText("Save"));

    await waitFor(() => {
      expect(mockCreate).toHaveBeenCalledWith({ name: "Salary", amount: 3000 });
      expect(onSuccess).toHaveBeenCalled();
    });
  });
});
```

---

## Rules Checklist

- [ ] Server Component by default — `"use client"` only when necessary
- [ ] All API responses typed with TypeScript interfaces
- [ ] No `any` types
- [ ] `API_BASE_URL` from `process.env`, never hardcoded
- [ ] `res.ok` checked in every fetch call with descriptive error message
- [ ] `@/` alias used in all imports
- [ ] Every new component has a unit test
- [ ] Every new service function has a unit test with mocked fetch
- [ ] Tailwind for all styling (no inline styles, no CSS modules unless justified)
