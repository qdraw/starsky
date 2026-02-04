import { OnMouseDownMouseAction } from "./on-mouse-down-mouse-action";
import { PositionObject } from "./pan-and-zoom-image";

describe("OnMoveMouseTouchAction", () => {
  it("should set values", () => {
    const setPosition = jest.fn();
    new OnMouseDownMouseAction(jest.fn(), {} as PositionObject, setPosition).onTouchStart({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ],
      preventDefault: jest.fn()
    } as unknown as TouchEvent);
    expect(setPosition).toHaveBeenCalledTimes(1);
    expect(setPosition).toHaveBeenCalledWith({ oldX: 1, oldY: 1 });
  });
});
