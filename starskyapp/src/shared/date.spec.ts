import { DifferenceInDate } from "./date";

describe("date", () => {
  describe("DifferenceInDate", () => {
    it("DifferenceInDate to be less than 80", () => {
      const result = DifferenceInDate(new Date().valueOf());
      expect(result).toBeLessThan(80);
    });
  });
});
