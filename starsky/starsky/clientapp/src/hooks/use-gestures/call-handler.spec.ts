import { ICurrentTouches } from "./ICurrentTouches.types";
import { IHandlers } from "./IHandlers.types";
import { callHandler } from "./call-handler";

describe("callHandler", () => {
  describe("callHandler", () => {
    it("should call test function", () => {
      const handlers = { test: jest.fn() } as unknown as IHandlers;
      callHandler("test", true as unknown as ICurrentTouches, handlers);

      expect((handlers as { test: jest.Mock }).test).toHaveBeenCalled();
    });

    it("should not call test function", () => {
      const handlers = { test: jest.fn() } as unknown as IHandlers;
      callHandler(undefined as unknown as string, true as unknown as ICurrentTouches, handlers);

      expect((handlers as { test: jest.Mock }).test).toHaveBeenCalledTimes(0);
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
