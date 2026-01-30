export function formatOffsetLabel(
  years: number,
  months: number,
  days: number,
  hours: number,
  minutes: number,
  seconds: number
) {
  const parts: string[] = [];
  if (years !== 0) parts.push(`${years > 0 ? "+" : ""}${years} years`);
  if (months !== 0) parts.push(`${months > 0 ? "+" : ""}${months} months`);
  if (days !== 0) parts.push(`${days > 0 ? "+" : ""}${days} days`);
  if (hours !== 0) parts.push(`${hours > 0 ? "+" : ""}${hours} hours`);
  if (minutes !== 0) parts.push(`${minutes > 0 ? "+" : ""}${minutes} minutes`);
  if (seconds !== 0) parts.push(`${seconds > 0 ? "+" : ""}${seconds} seconds`);

  if (parts.length === 0) return "No shift";
  return parts.join(", ");
}
