export function GetCookie(name: string): string {
  const regex = new RegExp("(^| )" + name + "=([^;]+)");
  const match = regex.exec(document.cookie);
  if (match) return match[2];
  return "X-XSRF-TOKEN";
}
