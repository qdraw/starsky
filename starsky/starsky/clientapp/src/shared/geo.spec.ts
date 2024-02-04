import { Geo } from "./geo";
describe("Geo", () => {
  const geo = new Geo();

  describe("Distance", () => {
    it("example distance not far", () => {
      const dis = geo.Distance([52.636206, 4.657292], [52.636118, 4.657241]);
      expect(dis).toBe(10);
    });

    it("example distance very far", () => {
      const dis = geo.Distance([-24.4051344, 128.2689035], [52.4841899, -71.071758]);
      expect(dis).toBe(16511007);
    });

    it("point 1 has wrong input", () => {
      let shouldGiveError: any;
      try {
        geo.Distance([1, 1, 1], []);
        throw Error("should fail");
      } catch (error) {
        shouldGiveError = error;
      }

      expect(shouldGiveError).toBeInstanceOf(Error);
      expect(shouldGiveError).toHaveProperty("message", "point 1 has wrong input");
    });

    it("point 2 has wrong input", () => {
      let shouldGiveError: any;
      try {
        geo.Distance([1, 1], []);
        throw Error("should fail");
      } catch (error) {
        shouldGiveError = error;
      }

      expect(shouldGiveError).toBeInstanceOf(Error);
      expect(shouldGiveError).toHaveProperty("message", "point 2 has wrong input");
    });
  });

  describe("Validate", () => {
    it("non valid lat", () => {
      const result = geo.Validate(44444, 15);

      expect(result).toBeFalsy();
    });

    it("non valid long", () => {
      const result = geo.Validate(51, 444444);

      expect(result).toBeFalsy();
    });

    it("valid undefined", () => {
      const result = geo.Validate(0, 0);

      expect(result).toBeTruthy();
    });

    it("valid example", () => {
      const result = geo.Validate(51, 15);

      expect(result).toBeTruthy();
    });

    it("valid example 2 (long)", () => {
      const result = geo.Validate(51.893458349589349855, 15.83450934590345345);

      expect(result).toBeTruthy();
    });
  });
});
