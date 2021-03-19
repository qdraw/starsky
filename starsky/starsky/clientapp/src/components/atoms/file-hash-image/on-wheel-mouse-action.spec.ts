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
});
