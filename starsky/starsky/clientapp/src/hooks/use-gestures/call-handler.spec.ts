import { callHandler } from "./call-handler";

describe("callHandler", () => {
  describe("callHandler", () => {
    it("should call test function", () => {
      let handlers = { test: jest.fn() } as any;
      callHandler("test", true as any, handlers);

      expect(handlers.test).toBeCalled();
    });
  });
});
