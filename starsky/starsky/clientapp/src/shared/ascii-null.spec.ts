import {
  AsciiNull,
  AsciiNullRegexEscaped,
  AsciiNullUrlEncoded
} from "./ascii-null";

describe("AsciiNull", () => {
  it("AsciiNull", () => {
    var result = AsciiNull();
    expect(result).toContain("\0");
  });
  it("AsciiNullRegexEscaped", () => {
    var result = AsciiNullRegexEscaped();
    expect(result).toContain("\\0");
  });
  it("AsciiNullUrlEncoded", () => {
    var result = AsciiNullUrlEncoded();
    expect(result).toContain("%00");
  });
});
