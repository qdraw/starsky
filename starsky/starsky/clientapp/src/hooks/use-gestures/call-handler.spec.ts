import { ICurrentTouches } from "./ICurrentTouches.types";
import { callHandler } from "./call-handler";

describe("callHandler", () => {
  describe("callHandler", () => {
    it("should call test function", () => {
      const handlers = { test: jest.fn() } as any;
      callHandler("test", true as any, handlers);

      expect(handlers.test).toBeCalled();
    });

    it("should not call test function", () => {
      const handlers = { test: jest.fn() } as any;
      callHandler(undefined as any, true as any, handlers);

      expect(handlers.test).toBeCalledTimes(0);
    });
  });

  it("should throw an error when the handler is missing", () => {
    const eventName = "someEvent";
    const event = {} as ICurrentTouches; // Provide an appropriate event object
    const handlers = undefined;

    expect(() => {
      callHandler(eventName, event, handlers);
    }).toThrow(`handler ${eventName} is missing`);
  });
});
