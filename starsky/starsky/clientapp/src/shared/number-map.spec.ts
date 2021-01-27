import { NumberMap } from "./number-map";

describe("NumberMap", () => {
  it("50 %", () => {
    const output = NumberMap(0.5, 0, 1, 0, 100);
    expect(output).toBe(50);
  });
});
