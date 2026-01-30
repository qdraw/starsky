export function formatOffsetLabel(
  years: number,
  months: number,
  days: number,
  hours: number,
  minutes: number,
  seconds: number
) {
  const values = [
    { value: years, label: "years" },
    { value: months, label: "months" },
    { value: days, label: "days" },
    { value: hours, label: "hours" },
    { value: minutes, label: "minutes" },
    { value: seconds, label: "seconds" }
  ];

  const parts = values
    .filter(({ value }) => value !== 0)
    .map(({ value, label }) => `${value > 0 ? "+" : ""}${value} ${label}`);

  return parts.length === 0 ? "No shift" : parts.join(", ");
}
