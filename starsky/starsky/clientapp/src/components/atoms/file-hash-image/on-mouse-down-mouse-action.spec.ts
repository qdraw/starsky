import { OnMouseDownMouseAction } from "./on-mouse-down-mouse-action";

describe("OnMoveMouseTouchAction", () => {
  it("should set values", () => {
    const setPosition = jest.fn();
    new OnMouseDownMouseAction(jest.fn(), {} as any, setPosition).onTouchStart({
      touches: [
        {
          clientX: 1,
          clientY: 1
        }
      ],
      preventDefault: jest.fn()
    } as any);
    expect(setPosition).toBeCalledTimes(1);
    expect(setPosition).toBeCalledWith({ oldX: 1, oldY: 1 });
  });
});
