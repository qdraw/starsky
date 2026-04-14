/**
 * Build a single-line JS call to resolve a folder picker Promise from the native bridge.
 *
 * Encodes values as described:
 * - requestId: JSON-encoded string
 * - path: JSON-encoded string or null
 * - bookmarkBase64: Base64 string JSON-encoded or null
 */
export function jsForResult(
  requestId: string,
  path?: string | null,
  bookmarkBase64?: string | null
): string {
  const idPart = JSON.stringify(requestId);
  const pathPart = path === undefined || path === null ? "null" : JSON.stringify(path);
  const bookmarkPart =
    bookmarkBase64 === undefined || bookmarkBase64 === null
      ? "null"
      : JSON.stringify(bookmarkBase64);

  return `window.__starskyNative._resolveFolderPick(${idPart},${pathPart},${bookmarkPart});`;
}

export default jsForResult;
