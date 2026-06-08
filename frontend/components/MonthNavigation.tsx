"use client";

interface MonthNavigationProps {
  year: number;
  month: number;
  onChange: (nextYear: number, nextMonth: number) => void;
  ariaLabel?: string;
}

function padMonth(month: number): string {
  return String(month).padStart(2, "0");
}

export function MonthNavigation({ year, month, onChange, ariaLabel }: MonthNavigationProps) {
  function goPrevious() {
    if (month === 1) {
      onChange(year - 1, 12);
      return;
    }

    onChange(year, month - 1);
  }

  function goNext() {
    if (month === 12) {
      onChange(year + 1, 1);
      return;
    }

    onChange(year, month + 1);
  }

  return (
    <div
      className="grid w-full grid-cols-[1fr_auto_1fr] items-center gap-3"
      aria-label={ariaLabel ?? "Navegacao mensal"}
    >
      <button
        type="button"
        onClick={goPrevious}
        className="justify-self-start rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 transition hover:bg-slate-100"
        aria-label="Mês Anterior"
      >
        Mês Anterior
      </button>

      <p
        className="justify-self-center text-center text-sm font-medium text-slate-800"
        aria-label="Mês e ano selecionados"
      >
        {padMonth(month)}/{year}
      </p>

      <button
        type="button"
        onClick={goNext}
        className="justify-self-end rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 transition hover:bg-slate-100"
        aria-label="Mês Próximo"
      >
        Mês Próximo
      </button>
    </div>
  );
}
