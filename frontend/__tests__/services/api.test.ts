import {
  createAccount,
  getAccounts,
  getDashboardSummary,
  markAccountAsPaid,
} from "@/services/api";

describe("services/api", () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  afterAll(() => {
    global.fetch = originalFetch;
  });

  it("getAccounts should request with month filter and return data", async () => {
    const payload = [
      { id: "1", name: "Internet", amount: 120.5, dueDate: "2026-05-01T00:00:00Z", paid: false },
    ];

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await getAccounts({ year: 2026, month: 5 });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts?year=2026&month=5"),
      { cache: "no-store" },
    );
  });

  it("markAccountAsPaid should call patch endpoint", async () => {
    const payload = { id: "1", name: "Internet", amount: 120.5, dueDate: "2026-05-01T00:00:00Z", paid: true };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await markAccountAsPaid("1");

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts/1/pay"),
      expect.objectContaining({ method: "PATCH" }),
    );
  });

  it("createAccount should call post endpoint", async () => {
    const payload = { id: "2", name: "Water", amount: 99.9, dueDate: "2026-05-05T00:00:00Z", paid: false };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await createAccount({
      name: "Water",
      amount: 99.9,
      dueDate: "2026-05-05T00:00:00Z",
    });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts"),
      expect.objectContaining({ method: "POST" }),
    );
  });

  it("getDashboardSummary should call dashboard endpoint with year and month", async () => {
    const payload = {
      year: 2026,
      month: 5,
      totalAmount: 1600,
      paidAmount: 1450,
      pendingAmount: 150,
      totalCount: 3,
      paidCount: 2,
      pendingCount: 1,
      chart: [
        { label: "Paid", amount: 1450, count: 2 },
        { label: "Pending", amount: 150, count: 1 },
      ],
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await getDashboardSummary({ year: 2026, month: 5 });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/dashboard/summary?year=2026&month=5"),
      { cache: "no-store" },
    );
  });
});
