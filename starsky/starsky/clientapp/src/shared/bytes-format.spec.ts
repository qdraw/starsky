import BytesFormat from "./bytes-format";

describe("BytesFormat", () => {
  it("0 bytes", () => {
    const result = BytesFormat(0);
    expect(result).toBeFalsy();
  });

  it("undefined", () => {
    const result = BytesFormat(undefined as any);
    expect(result).toBeFalsy();
  });

  it("null", () => {
    const result = BytesFormat(null as any);
    expect(result).toBeFalsy();
  });

  it("1 bytes", () => {
    const result = BytesFormat(1);
    expect(result).toBe("1 Bytes");
  });

  it("25508608 bytes", () => {
    const result = BytesFormat(25508608);
    expect(result).toBe(`${(24.33).toLocaleString()} MB`);
  });

  it("4632269 bytes", () => {
    const result = BytesFormat(4632269);
    expect(result).toBe(`${(4.42).toLocaleString()} MB`);
  });

  it("81328 bytes", () => {
    const result = BytesFormat(81328);
    expect(result).toBe(`${(79.42).toLocaleString()} KB`);
  });

  it("81328 bytes and 0 decimals", () => {
    const result = BytesFormat(81328, 0);
    expect(result).toBe(`79 KB`);
  });

  it("81328 bytes and minus 5 decimals as 0 decimals", () => {
    const result = BytesFormat(81328, -5);
    expect(result).toBe(`79 KB`);
  });
});
