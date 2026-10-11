export function IsRelativeUrl(url: string): boolean {
  if (typeof url !== "string") return false;

  // Browsers treat `//host`, `\\host` and `/\host` as protocol-relative (off-site) urls,
  // and silently drop tabs/newlines, so reject all control characters up front.
  // eslint-disable-next-line no-control-regex
  if (/[\u0000-\u001F\u007F]/.test(url)) return false;
  if (/^[/\\]{2}/.test(url) || url.startsWith("/\\") || url.startsWith("\\")) return false;

  // Any scheme (javascript:, data:, vbscript:, http: ...) before the first / ? or #
  if (/^[a-z][a-z0-9+.-]*:/i.test(url)) return false;

  // Regular expression pattern to match relative URLs
  const relativeUrlRegex = /^(?!https?:\/\/)(?![^/]*\.[^/.]+\/)[^?#\s]+(?:\?[^#\s]*)?(?:#\S*)?$/;

  // Check if the URL matches the relative URL pattern
  return relativeUrlRegex.test(url);
}
