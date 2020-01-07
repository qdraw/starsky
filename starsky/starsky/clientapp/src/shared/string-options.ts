/**
 * strings
 */
export class StringOptions {
  public LimitLength(input: string, lenght: number) {
    if (input.length <= lenght) return input;
    return input.substr(0, lenght) + "..."
  }
}

