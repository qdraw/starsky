import {
  AsciiNull,
  AsciiNullRegexEscaped,
  AsciiNullUrlEncoded
} from "./ascii-null";

describe("AsciiNull", () => {
  it("AsciiNull", () => {
    const result = AsciiNull();
    expect(result).toContain("\0");
  });
  it("AsciiNullRegexEscaped", () => {
    const result = AsciiNullRegexEscaped();
    expect(result).toContain("\\0");
  });
  it("AsciiNullUrlEncoded", () => {
    const result = AsciiNullUrlEncoded();
    expect(result).toContain("%00");
  });
});
