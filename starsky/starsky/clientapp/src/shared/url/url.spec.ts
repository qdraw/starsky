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
});
