import { debounce } from "./debounce";

describe("debounce", () => {
  describe("debounce", () => {
    it("should call test function", () => {
      jest.useFakeTimers();

      const funcSpy = jest.fn();
      const dBounce = debounce(funcSpy, 10);

      dBounce();
      jest.advanceTimersByTime(20);

      expect(funcSpy).toBeCalled();

      jest.useFakeTimers();
    });

    it("should not call test function", () => {
      const funcSpy = jest.fn();
      const dBounce = debounce(funcSpy, 10);

      dBounce();

      expect(funcSpy).toBeCalledTimes(0);
    });
  });
});
