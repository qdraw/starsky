export function AsciiNull() {
  return "\0";
}

export function AsciiNullRegexEscaped() {
  return String.raw`\0`;
}

export function AsciiNullUrlEncoded() {
  return "%00";
}
