import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { PeopleManager } from "@/components/PeopleManager";
import * as api from "@/services/api";

jest.mock("@/services/api", () => ({
  ...jest.requireActual("@/services/api"),
  createPerson: jest.fn(),
  deletePerson: jest.fn(),
}));

describe("PeopleManager", () => {
  const token = "backend-token";

  const initialPeople = [
    {
      id: "person-1",
      name: "Ana",
      type: "Guest",
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
      type: "Guest",
      createdAtUtc: "2026-05-02T00:00:00Z",
      deletedAtUtc: null,
    });

    render(<PeopleManager initialPeople={initialPeople} token={token} />);

    fireEvent.change(screen.getByLabelText("Nome da pessoa"), { target: { value: "Bruno" } });
    fireEvent.click(screen.getByRole("button", { name: "Adicionar pessoa" }));

    await waitFor(() => {
      expect(api.createPerson).toHaveBeenCalledWith("Bruno", token);
      expect(screen.getByText("Bruno")).toBeInTheDocument();
    });
  });

  it("deletes an existing person", async () => {
    (api.deletePerson as jest.Mock).mockResolvedValue(undefined);

    render(<PeopleManager initialPeople={initialPeople} token={token} />);

    fireEvent.click(screen.getByRole("button", { name: "Excluir" }));

    await waitFor(() => {
      expect(api.deletePerson).toHaveBeenCalledWith("person-1", token);
      expect(screen.queryByText("Ana")).not.toBeInTheDocument();
    });
  });

  it("does not allow deleting owner person", () => {
    render(
      <PeopleManager
        initialPeople={[
          {
            id: "owner-1",
            name: "Owner",
            type: "Owner",
            createdAtUtc: "2026-05-01T00:00:00Z",
            deletedAtUtc: null,
          },
        ]}
        token={token}
      />,
    );

    expect(screen.getByText("Não removível")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Excluir" })).not.toBeInTheDocument();
  });
});
