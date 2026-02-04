import { OnMoveMouseTouchAction } from "./on-move-mouse-touch-action";
import { PositionObject } from "./pan-and-zoom-image";

describe("OnMoveMouseTouchAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should not set when input has no touches", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(
      null as unknown as boolean,
      setPosition,
      null as unknown as PositionObject
    ).touchMove({} as unknown as TouchEvent);
    expect(setPosition).toHaveBeenCalledTimes(0);
  });

  it("should not set when input has no panning", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(
      null as unknown as boolean,
      setPosition,
      null as unknown as PositionObject
    ).touchMove({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ]
    } as unknown as TouchEvent);
    expect(setPosition).toHaveBeenCalledTimes(0);
  });

  it("should setPosition with clientX and y 1 and values", () => {
    const setPosition = jest.fn();
    new OnMoveMouseTouchAction(true, setPosition, { y: 1 } as PositionObject).touchMove({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ]
    } as unknown as TouchEvent);
    expect(setPosition).toHaveBeenCalledTimes(1);
    expect(setPosition).toHaveBeenCalledWith({ oldX: 1, oldY: 1, x: NaN, y: NaN });
  });

  it("should not setPosition with clientX and y 0 and values", () => {
    const setPosition = jest.fn();
    // .. y = 0 should not set
    new OnMoveMouseTouchAction(true, setPosition, { y: 0 } as PositionObject).touchMove({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ]
    } as unknown as TouchEvent);
    expect(setPosition).toHaveBeenCalledTimes(0);
  });
});
