import { Geo } from './geo';
describe("Geo", () => {

  var geo = new Geo();

  describe("Distance", () => {

    it("example distance not far", () => {
      var dis = geo.Distance([52.636206, 4.657292], [52.636118, 4.657241]);
      expect(dis).toBe(10)
    });

    it("example distance very far", () => {
      var dis = geo.Distance([-24.4051344, 128.2689035], [52.4841899, -71.071758]);
      expect(dis).toBe(16511007)
    });


    it("point 1 has wrong input", () => {
      try {
        geo.Distance([1, 1, 1], []);
        throw Error("should fail");
      } catch (error) {
        expect(error).toBeInstanceOf(Error);
        expect(error).toHaveProperty('message', 'point 1 has wrong input');
      }
    });

    it("point 2 has wrong input", () => {
      try {
        geo.Distance([1, 1], []);
        throw Error("should fail");
      } catch (error) {
        expect(error).toBeInstanceOf(Error);
        expect(error).toHaveProperty('message', 'point 2 has wrong input');
      }
    });

  });
});