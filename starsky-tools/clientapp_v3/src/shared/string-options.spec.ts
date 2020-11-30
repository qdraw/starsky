import { StringOptions } from './string-options';

describe("StringOptions", () => {
  var stringOptions = new StringOptions();

  describe("LimitLength", () => {

    it("short text", () => {
      var output = stringOptions.LimitLength("abcd", 2);
      expect(output).toBe('ab…');
    });

    it("long text", () => {
      var output = stringOptions.LimitLength("abcd", 10);
      expect(output).toBe('abcd');
    });
  });
});