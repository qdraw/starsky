import { StringOptions } from "./string-options";

describe("StringOptions", () => {
  const stringOptions = new StringOptions();

  describe("LimitLength", () => {
    it("short text", () => {
      const output = stringOptions.LimitLength("abcd", 2);
      expect(output).toBe("ab…");
    });

    it("long text", () => {
      const output = stringOptions.LimitLength("abcd", 10);
      expect(output).toBe("abcd");
    });
  });
});
