import { render, screen } from "@testing-library/react";
import { IconBar } from "@/components/IconBar";

jest.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

describe("IconBar", () => {
  it("renders icon buttons and pin toggle", () => {
    const onIconHover = jest.fn();
    const onIconLeave = jest.fn();
    const onTogglePin = jest.fn();

    render(
      <IconBar
        onIconHover={onIconHover}
        onIconLeave={onIconLeave}
        isPinned={false}
        onTogglePin={onTogglePin}
      />,
    );

    expect(screen.getByRole("complementary", { name: /barra de ícones/i })).toBeInTheDocument();
    
    const pinButton = screen.getByRole("button", { name: /afixar menu/i });
    expect(pinButton).toBeInTheDocument();

    expect(screen.getByRole("button", { name: /visao geral/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /gestao financeira/i })).toBeInTheDocument();
  });

  it("calls onTogglePin when pin button is clicked", () => {
    const onTogglePin = jest.fn();

    render(
      <IconBar
        onIconHover={jest.fn()}
        onIconLeave={jest.fn()}
        isPinned={false}
        onTogglePin={onTogglePin}
      />,
    );

    const pinButton = screen.getByRole("button", { name: /afixar menu/i });
    pinButton.click();

    expect(onTogglePin).toHaveBeenCalledTimes(1);
  });

  it("shows pinned icon when isPinned is true", () => {
    render(
      <IconBar
        onIconHover={jest.fn()}
        onIconLeave={jest.fn()}
        isPinned={true}
        onTogglePin={jest.fn()}
      />,
    );

    expect(screen.getByRole("button", { name: /desafixar menu/i })).toBeInTheDocument();
  });
});

