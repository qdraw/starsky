import getTextLength from "./get-text-length";

describe("getTextLength", () => {
  it("returns length for string", () => {
    expect(getTextLength("hello")).toBe(5);
  });

  it("returns length for number", () => {
    expect(getTextLength(12345)).toBe(5);
  });

  it("returns 0 for null or undefined", () => {
    expect(getTextLength(null)).toBe(0);
    expect(getTextLength(undefined)).toBe(0);
  });

  it("returns length for array of strings and numbers", () => {
    expect(getTextLength(["hi", 123, "a"])).toBe(2 + 3 + 1);
  });

  it("returns length for React element with string children", () => {
    expect(getTextLength(<span>hello</span>)).toBe(5);
  });

  it("returns length for React element with number children", () => {
    expect(getTextLength(<span>{123}</span>)).toBe(3);
  });

  it("returns length for nested React elements", () => {
    expect(
      getTextLength(
        <div>
          <span>hi</span>
          <span>there</span>
        </div>
      )
    ).toBe(7);
  });

  it("returns length for array of React elements", () => {
    expect(getTextLength([<span key="1">foo</span>, <span key="2">bar</span>])).toBe(6);
  });

  it("returns 0 for empty array", () => {
    expect(getTextLength([])).toBe(0);
  });

  it("returns 0 for React element with no children", () => {
    expect(getTextLength(<span />)).toBe(0);
  });
});
