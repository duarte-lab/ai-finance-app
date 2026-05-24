import {
  createPerson,
  createMonthlyClosing,
  reopenMonthlyClosing,
  createAccount,
  getDueNotifications,
  getAccounts,
  getDashboardSummary,
  getPeople,
  markAccountAsPaid,
  updateAccount,
  updateAccountDivisionParticipation,
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
      {
        id: "1",
        name: "Internet",
        amount: 120.5,
        dueDate: "2026-05-01T00:00:00Z",
        createdAtUtc: "2026-04-01T00:00:00Z",
        paid: false,
        participatesInDivision: false,
        participants: [],
      },
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
    const payload = {
      id: "1",
      name: "Internet",
      amount: 120.5,
      dueDate: "2026-05-01T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: true,
      participatesInDivision: false,
      participants: [],
    };

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
    const payload = {
      id: "2",
      name: "Water",
      amount: 99.9,
      dueDate: "2026-05-05T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: false,
      participants: [],
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await createAccount({
      name: "Water",
      amount: 99.9,
      dueDate: "2026-05-05T00:00:00Z",
      participatesInDivision: false,
      participants: [],
    });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts"),
      expect.objectContaining({ method: "POST" }),
    );
  });

  it("updateAccount should call put endpoint", async () => {
    const payload = {
      id: "2",
      name: "Water updated",
      amount: 100,
      dueDate: "2026-05-05T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: true,
      participants: [],
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await updateAccount("2", {
      name: "Water updated",
      amount: 100,
      dueDate: "2026-05-05T00:00:00Z",
      paid: false,
      participatesInDivision: true,
      participants: [],
    });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts/2"),
      expect.objectContaining({ method: "PUT" }),
    );
  });

  it("updateAccountDivisionParticipation should call patch endpoint", async () => {
    const payload = {
      id: "2",
      name: "Water",
      amount: 100,
      dueDate: "2026-05-05T00:00:00Z",
      createdAtUtc: "2026-04-01T00:00:00Z",
      paid: false,
      participatesInDivision: true,
      participants: [],
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await updateAccountDivisionParticipation("2", true);

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/accounts/2/division-participation"),
      expect.objectContaining({ method: "PATCH" }),
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

  it("createMonthlyClosing should call closing endpoint", async () => {
    const payload = {
      id: "closing-1",
      year: 2026,
      month: 5,
      totalAmount: 1500,
      amountPerPerson: 750,
      accountCount: 2,
      participantCount: 2,
      closedAtUtc: "2026-05-25T00:00:00Z",
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await createMonthlyClosing({
      year: 2026,
      month: 5,
      accountIds: ["a1", "a2"],
      participants: ["Ana", "Bruno"],
    });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/closing"),
      expect.objectContaining({ method: "POST" }),
    );
  });

  it("getPeople should call people endpoint", async () => {
    const payload = [{ id: "person-1", name: "Ana", createdAtUtc: "2026-05-05T00:00:00Z" }];

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await getPeople();

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/people"),
      { cache: "no-store" },
    );
  });

  it("createPerson should call people post endpoint", async () => {
    const payload = { id: "person-2", name: "Bruno", createdAtUtc: "2026-05-05T00:00:00Z" };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await createPerson("Bruno");

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/people"),
      expect.objectContaining({ method: "POST" }),
    );
  });

  it("getDueNotifications should call notifications endpoint", async () => {
    const payload = [
      {
        accountId: "account-1",
        accountName: "Rent",
        dueDateUtc: "2026-05-23T00:00:00Z",
        daysUntilDue: 0,
        type: "DueToday",
        message: "A conta 'Rent' vence hoje.",
      },
    ];

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await getDueNotifications();

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/notifications/due"),
      { cache: "no-store" },
    );
  });

  it("reopenMonthlyClosing should call reopen endpoint", async () => {
    const payload = {
      id: "closing-1",
      year: 2026,
      month: 5,
      totalAmount: 1500,
      amountPerPerson: 750,
      accountCount: 2,
      participantCount: 2,
      closedAtUtc: "2026-05-25T00:00:00Z",
      isReopened: true,
      reopenedAtUtc: "2026-05-26T00:00:00Z",
    };

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => payload,
    });

    const result = await reopenMonthlyClosing({ year: 2026, month: 5 });

    expect(result).toEqual(payload);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/closing/reopen"),
      expect.objectContaining({ method: "POST" }),
    );
  });
});
