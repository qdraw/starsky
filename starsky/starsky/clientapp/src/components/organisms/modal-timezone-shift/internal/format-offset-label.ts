export interface IFormatOffsetLabelProps {
  label: string;
  value: number;
}

export function formatOffsetLabel(
  years: IFormatOffsetLabelProps,
  months: IFormatOffsetLabelProps,
  days: IFormatOffsetLabelProps,
  hours: IFormatOffsetLabelProps,
  minutes: IFormatOffsetLabelProps,
  seconds: IFormatOffsetLabelProps
) {
  const values = [
    { value: years.value, label: years.label },
    { value: months.value, label: months.label },
    { value: days.value, label: days.label },
    { value: hours.value, label: hours.label },
    { value: minutes.value, label: minutes.label },
    { value: seconds.value, label: seconds.label }
  ];

  const parts = values
    .filter(({ value }) => value !== 0)
    .map(({ value, label }) => `${value > 0 ? "+" : ""}${value} ${label}`);

  return parts.length === 0 ? "No shift" : parts.join(", ");
}
