import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { PeopleManager } from "@/components/PeopleManager";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  createPerson: jest.fn(),
  deletePerson: jest.fn(),
}));

describe("PeopleManager", () => {
  const initialPeople = [
    {
      id: "person-1",
      name: "Ana",
      createdAtUtc: "2026-05-01T00:00:00Z",
      deletedAtUtc: null,
    },
  ];

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("creates a new person", async () => {
    (api.createPerson as jest.Mock).mockResolvedValue({
      id: "person-2",
      name: "Bruno",
      createdAtUtc: "2026-05-02T00:00:00Z",
      deletedAtUtc: null,
    });

    render(<PeopleManager initialPeople={initialPeople} />);

    fireEvent.change(screen.getByLabelText("Nome da pessoa"), { target: { value: "Bruno" } });
    fireEvent.click(screen.getByRole("button", { name: "Adicionar pessoa" }));

    await waitFor(() => {
      expect(api.createPerson).toHaveBeenCalledWith("Bruno");
      expect(screen.getByText("Bruno")).toBeInTheDocument();
    });
  });

  it("deletes an existing person", async () => {
    (api.deletePerson as jest.Mock).mockResolvedValue(undefined);

    render(<PeopleManager initialPeople={initialPeople} />);

    fireEvent.click(screen.getByRole("button", { name: "Excluir" }));

    await waitFor(() => {
      expect(api.deletePerson).toHaveBeenCalledWith("person-1");
      expect(screen.queryByText("Ana")).not.toBeInTheDocument();
    });
  });
});
