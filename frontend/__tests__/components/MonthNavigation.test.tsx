import { fireEvent, render, screen } from "@testing-library/react";
import { MonthNavigation } from "@/components/MonthNavigation";

describe("MonthNavigation", () => {
  it("renders selected month/year", () => {
    render(<MonthNavigation year={2026} month={5} onChange={() => undefined} />);

    expect(screen.getByLabelText(/mês e ano selecionados/i)).toHaveTextContent("05/2026");
  });

  it("navigates to previous month", () => {
    const onChange = jest.fn();

    render(<MonthNavigation year={2026} month={5} onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: /mês anterior/i }));

    expect(onChange).toHaveBeenCalledWith(2026, 4);
  });

  it("navigates to next month", () => {
    const onChange = jest.fn();

    render(<MonthNavigation year={2026} month={5} onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: /mês próximo/i }));

    expect(onChange).toHaveBeenCalledWith(2026, 6);
  });

  it("handles year boundary when going previous from january", () => {
    const onChange = jest.fn();

    render(<MonthNavigation year={2026} month={1} onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: /mês anterior/i }));

    expect(onChange).toHaveBeenCalledWith(2025, 12);
  });

  it("handles year boundary when going next from december", () => {
    const onChange = jest.fn();

    render(<MonthNavigation year={2026} month={12} onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: /mês próximo/i }));

    expect(onChange).toHaveBeenCalledWith(2027, 1);
  });
});
