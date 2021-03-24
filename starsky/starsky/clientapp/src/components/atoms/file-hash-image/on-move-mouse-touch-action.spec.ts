import { OnMoveMouseTouchAction } from "./on-move-mouse-touch-action";

describe("OnMoveMouseTouchAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should not set when input has no touches", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(null as any, setPosition, null as any).touchMove(
      {} as any
    );
    expect(setPosition).toBeCalledTimes(0);
  });

  it("should not set when input has no panning", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(null as any, setPosition, null as any).touchMove(
      {
        touches: [
          {
            clientX: 1,
            clientY: 1
          }
        ]
      } as any
    );
    expect(setPosition).toBeCalledTimes(0);
  });

  it("should setPosition with clientX and y 1 and values", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(true, setPosition, { y: 1 } as any).touchMove({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ]
    } as any);
    expect(setPosition).toBeCalledTimes(1);
    expect(setPosition).toBeCalledWith({ oldX: 1, oldY: 1, x: NaN, y: NaN });
  });

  it("should not setPosition with clientX and y 0 and values", () => {
    const setPosition = jest.fn();
    // .. y = 0 should not set
    new OnMoveMouseTouchAction(true, setPosition, { y: 0 } as any).touchMove({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ]
    } as any);
    expect(setPosition).toBeCalledTimes(0);
  });
});
