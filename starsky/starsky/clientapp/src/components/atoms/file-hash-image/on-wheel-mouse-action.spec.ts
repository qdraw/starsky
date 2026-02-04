import { OnWheelMouseAction } from "./on-wheel-mouse-action";
import { ImageObject, PositionObject } from "./pan-and-zoom-image";

describe("OnWheelMouseAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should not set when current is null and delta exist", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      null as unknown as ImageObject,
      setPosition,
      null as unknown as PositionObject,
      { current: null } as unknown as React.RefObject<HTMLButtonElement>,
      null as unknown as (z: number) => void
    ).onWheel({
      deltaY: 1
    } as unknown as React.WheelEvent<HTMLButtonElement>);
    expect(setPosition).toHaveBeenCalledTimes(0);
  });

  it("should not set when current is null and delta not exist", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      null as unknown as ImageObject,
      setPosition,
      null as unknown as PositionObject,
      { current: 1 } as unknown as React.RefObject<HTMLButtonElement>,
      null as unknown as (z: number) => void
    ).onWheel({
      deltaY: 0 // <= not exist
    } as unknown as React.WheelEvent<HTMLButtonElement>);
    expect(setPosition).toHaveBeenCalledTimes(0);
  });

  it("zoom - assume middle of image when not passing in any values", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      { height: 40, width: 40 }, // image
      setPosition,
      { x: 0, y: 0, z: 0 } as PositionObject, // position
      {
        current: {
          getBoundingClientRect: () => {
            return {
              x: 30,
              width: 30
            };
          }
        }
      } as unknown as React.RefObject<HTMLButtonElement>,
      jest.fn()
    ).zoom(-3);
    expect(setPosition).toHaveBeenCalledTimes(1);
    // if not enabled is x": 4.5,
    expect(setPosition).toHaveBeenCalledWith({ x: 3, y: 0, z: 0 });
  });

  it("zoom - ignore horizontal middle of image when not passing in any values", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      { height: 40, width: 40 }, // image
      setPosition,
      { x: 0, y: 0, z: 0 } as PositionObject, // position
      {
        current: {
          getBoundingClientRect: () => {
            return {
              x: 30,
              width: 30
            };
          }
        }
      } as unknown as React.RefObject<HTMLButtonElement>,
      jest.fn()
    ).zoom(-3, 99);
    expect(setPosition).toHaveBeenCalledTimes(1);
    expect(setPosition).toHaveBeenCalledWith({
      x: -5.4,
      y: -8.4,
      z: 0
    });
  });

  it("zoom - ignore vertical middle of image when not passing in any values", () => {
    const setPosition = jest.fn();
    new OnWheelMouseAction(
      { height: 40, width: 40 }, // image
      setPosition,
      { x: 0, y: 0, z: 0 } as PositionObject, // position
      {
        current: {
          getBoundingClientRect: () => {
            return {
              y: 30,
              width: 30
            };
          }
        }
      } as unknown as React.RefObject<HTMLButtonElement>,
      jest.fn()
    ).zoom(-3, 99);
    expect(setPosition).toHaveBeenCalledTimes(1);
    expect(setPosition).toHaveBeenCalledWith({
      x: -8.4,
      y: -5.4,
      z: 0
    });
  });
});
