import { OnWheelMouseAction } from "./on-wheel-mouse-action";

describe("OnWheelMouseAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should not set when current is null and delta exist", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      null as any,
      setPosition,
      null as any,
      { current: null } as any,
      null as any
    ).onWheel({
      deltaY: 1
    } as any);
    expect(setPosition).toBeCalledTimes(0);
  });

  it("should not set when current is null and delta not exist", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      null as any,
      setPosition,
      null as any,
      { current: 1 } as any,
      null as any
    ).onWheel({
      deltaY: 0 // <= not exist
    } as any);
    expect(setPosition).toBeCalledTimes(0);
  });

  it("zoom - assume middle of image when not passing in any values", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      { height: 40, width: 40 }, // image
      setPosition,
      { x: 0, y: 0, z: 0 } as any, // position
      {
        current: {
          getBoundingClientRect: () => {
            return {
              x: 30,
              width: 30
            };
          }
        }
      } as any,
      jest.fn()
    ).zoom(-3);
    expect(setPosition).toBeCalledTimes(1);
    // if not enabled is x": 4.5,
    expect(setPosition).toBeCalledWith({ x: 3, y: NaN, z: 0 });
  });
});
