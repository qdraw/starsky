import { MutableRefObject } from "react";
import { ICurrentTouches } from "./ICurrentTouches.types";
import { Pointer } from "./pointer";
import { getAngleDeg, getDistance } from "./use-gestures";

export const getCurrentTouches = (
  originalEvent: globalThis.TouchEvent,
  touches: TouchList,
  prevTouch: ICurrentTouches | null,
  initialTouches: MutableRefObject<ICurrentTouches>
): ICurrentTouches => {
  const firstTouch = initialTouches.current;

  if (touches.length === 2) {
    const pointer1 = new Pointer(touches[0]);
    const pointer2 = new Pointer(touches[1]);

    const distance = getDistance(pointer1, pointer2);

    return {
      preventDefault: originalEvent.preventDefault,
      stopPropagation: originalEvent.stopPropagation,
      pointers: [pointer1, pointer2],
      delta: prevTouch ? distance - prevTouch.distance : 0,
      scale: firstTouch ? distance / firstTouch.distance : 1,
      distance,
      angleDeg: getAngleDeg(pointer1, pointer2)
    };
  }

  // When single touch
  const pointer = new Pointer(touches[0]);

  return {
    preventDefault: originalEvent.preventDefault,
    stopPropagation: originalEvent.stopPropagation,
    ...pointer,
    deltaX: prevTouch && prevTouch.x ? pointer.x - prevTouch.x : 0,
    deltaY: prevTouch && prevTouch.y ? pointer.y - prevTouch.y : 0,
    delta: prevTouch ? getDistance(pointer, prevTouch) : 0,
    distance: firstTouch ? getDistance(pointer, firstTouch) : 0,
    angleDeg: prevTouch ? getAngleDeg(pointer, prevTouch) : 0
  };
};
