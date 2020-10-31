import BytesFormat from './bytes-format';

describe("BytesFormat", () => {

  it("0 bytes", () => {
    var result = BytesFormat(0);
    expect(result).toBeFalsy()
  });

  it("undefined", () => {
    var result = BytesFormat(undefined as any);
    expect(result).toBeFalsy()
  });

  it("null", () => {
    var result = BytesFormat(null as any);
    expect(result).toBeFalsy()
  });

  it("1 bytes", () => {
    var result = BytesFormat(1);
    expect(result).toBe('1 Bytes')
  });

  it("25508608 bytes", () => {
    var result = BytesFormat(25508608);
    expect(result).toBe(`${(24.33).toLocaleString()} MB`)
  });

  it("4632269 bytes", () => {
    var result = BytesFormat(4632269);
    expect(result).toBe(`${(4.42).toLocaleString()} MB`)
  });

  it("81328 bytes", () => {
    var result = BytesFormat(81328);
    expect(result).toBe(`${(79.42).toLocaleString()} KB`)
  });

  it("81328 bytes and 0 decimals", () => {
    var result = BytesFormat(81328, 0);
    expect(result).toBe(`79 KB`)
  });

  it("81328 bytes and minus 5 decimals as 0 decimals", () => {
    var result = BytesFormat(81328, -5);
    expect(result).toBe(`79 KB`)
  });
});
