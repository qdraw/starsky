export function formatOffsetLabel(
  yearLabel: string,
  years: number,
  monthLabel: string,
  months: number,
  dayLabel: string,
  days: number,
  hourLabel: string,
  hours: number,
  minuteLabel: string,
  minutes: number,
  secondLabel: string,
  seconds: number
) {
  const values = [
    { value: years, label: yearLabel },
    { value: months, label: monthLabel },
    { value: days, label: dayLabel },
    { value: hours, label: hourLabel },
    { value: minutes, label: minuteLabel },
    { value: seconds, label: secondLabel }
  ];

  const parts = values
    .filter(({ value }) => value !== 0)
    .map(({ value, label }) => `${value > 0 ? "+" : ""}${value} ${label}`);

  return parts.length === 0 ? "No shift" : parts.join(", ");
}
