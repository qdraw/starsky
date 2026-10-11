import { IsRelativeUrl } from "./url";

describe("isRelativeUrl function", () => {
  it("should return true for relative URLs", () => {
    expect(IsRelativeUrl("/path/to/resource")).toBe(true);
    expect(IsRelativeUrl("path/to/resource")).toBe(true);
    expect(IsRelativeUrl("path/to/resource?query=test")).toBe(true);
    expect(IsRelativeUrl("path/to/resource#section")).toBe(true);
    expect(IsRelativeUrl("path/to/resource?query=test#section")).toBe(true);
  });

  it("should return false for absolute URLs", () => {
    expect(IsRelativeUrl("http://example.com")).toBe(false);
    expect(IsRelativeUrl("https://example.com")).toBe(false);
  });

  it("should return false for protocol-relative and scheme urls", () => {
    for (const url of [
      "//evil.com",
      "//evil.com/path",
      "\\\\evil.com",
      "/\\evil.com",
      "\\evil.com",
      "javascript:alert(1)",
      "JaVaScRiPt:alert(1)",
      "data:text/html,<script>alert(1)</script>",
      "vbscript:x",
      "/\t/evil.com",
      "java\nscript:alert(1)"
    ]) {
      expect(IsRelativeUrl(url)).toBe(false);
    }
  });

  it("should keep accepting legitimate relative paths", () => {
    expect(IsRelativeUrl("/?f=/")).toBe(true);
    expect(IsRelativeUrl("/starsky/?f=/a/b.jpg")).toBe(true);
    expect(IsRelativeUrl("/search?t=a:b")).toBe(true);
  });
});
