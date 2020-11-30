/**
 * strings
 */
export class StringOptions {
  public LimitLength(input: string, length: number) {
    if (input.length <= length) return input;
    return input.substr(0, length) + "…" // &hellip;	HORIZONTAL ELLIPSIS
  }
}

