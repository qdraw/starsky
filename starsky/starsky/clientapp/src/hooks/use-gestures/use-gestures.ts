import {
  MutableRefObject,
  RefObject,
  useEffect,
  useRef,
  useState
} from "react";
import { callHandler } from "./call-handler";
import { ICurrentTouches } from "./ICurrentTouches.types";
import { IHandlers } from "./IHandlers.types";
import { Pointer } from "./pointer";

const debounce = (func: any, wait: number) => {
  let timeout: any;
  return function (...args: any) {
    // @ts-ignore
    const context = this;
    clearTimeout(timeout);
    timeout = setTimeout(() => func.apply(context, args), wait);
  };
};

/**
 * Calc the distance
 * @param {{x: number, y: number}} p1
 * @param {{x: number, y: number}} p2
 */
export const getDistance = (p1: Pointer, p2: Pointer | ICurrentTouches) => {
  if (p2.x === undefined || p2.y === undefined) {
    p2.x = 0;
    p2.y = 0;
  }
  const powX = Math.pow(p1.x - p2.x, 2);
  const powY = Math.pow(p1.y - p2.y, 2);

  return Math.sqrt(powX + powY);
};

/**
 *
 * @param {{x: number, y: number}} p1
 * @param {{x: number, y: number}} p2
 */
export const getAngleDeg = (p1: Pointer, p2: Pointer | ICurrentTouches) => {
  if (p2.x === undefined || p2.y === undefined) {
    p2.x = 0;
    p2.y = 0;
  }
  return (Math.atan2(p1.y - p2.y, p1.x - p2.x) * 180) / Math.PI;
};

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

/**
 * 
 * @param {Object} ref React ref object
 * @param {{   
    onPanStart: function,
    onPanMove: function,
    onSwipeLeft: function,
    onSwipeRight: function,
    onSwipeUp: function,
    onSwipeDown: function,
    onPanEnd: function,
    onSwipeLeftEnd: function,
    onSwipeRightEnd: function,
    onSwipeUpEnd: function,
    onSwipeDownEnd: function,
    onPinchStart: function,
    onPinchChanged: function,
    onPinchEnd: function,
    }} handlers 
 * @param {{
  minDelta: number
}} options 
*/
export function useGestures(
  ref: RefObject<HTMLElement>,
  handlers: IHandlers,
  options = {
    minDelta: 30
  }
) {
  const [touches, setTouches] = useState({} as ICurrentTouches);
  const [gesture, setGesture] = useState("");

  const initialTouches = useRef({} as ICurrentTouches);

  useEffect(() => {
    const element = ref.current;

    const handleTouchStart = (event: globalThis.TouchEvent) => {
      const currentTouches = getCurrentTouches(
        event,
        event.touches,
        null,
        initialTouches
      );
      setTouches(currentTouches);
      initialTouches.current = currentTouches;

      console.log("event.touches");

      console.log(event.touches.length);

      if (event.touches.length === 2) {
        callHandler("onPinchStart", currentTouches, handlers);
      } else {
        callHandler("onPanStart", currentTouches, handlers);
      }
    };

    const handleTouchMove = (event: globalThis.TouchEvent) => {
      const currentTouches = getCurrentTouches(
        event,
        event.touches,
        touches,
        initialTouches
      );
      setTouches(currentTouches);

      if (event.touches.length === 2) {
        callHandler("onPinchChanged", currentTouches, handlers);
        return;
      }

      callHandler("onPanMove", currentTouches, handlers);

      let eventName, theGesture;

      if (
        currentTouches.deltaX === undefined ||
        currentTouches.deltaY === undefined
      ) {
        return;
      }

      if (
        Math.abs(currentTouches.deltaX) >= options.minDelta &&
        Math.abs(currentTouches.deltaY) < options.minDelta
      ) {
        if (currentTouches.deltaX < 0) {
          eventName = "onSwipeLeft";
          theGesture = "swipeLeft";
        } else {
          eventName = "onSwipeRight";
          theGesture = "swipeRight";
        }
      } else if (
        Math.abs(currentTouches.deltaX) < options.minDelta &&
        Math.abs(currentTouches.deltaY) >= options.minDelta
      ) {
        if (currentTouches.deltaY < 0) {
          eventName = "onSwipeUp";
          theGesture = "swipeUp";
        } else {
          eventName = "onSwipeDown";
          theGesture = "swipeDown";
        }
      } else {
        theGesture = "";
      }

      if (eventName) {
        debounce(
          (eventName: string, touches: ICurrentTouches, theGesture: string) => {
            callHandler(eventName, touches, handlers);
            setGesture(theGesture);
          },
          100
        )(eventName, touches, theGesture);
      }
    };

    const handleTouchEnd = (event: globalThis.TouchEvent) => {
      const currentTouches = getCurrentTouches(
        event,
        event.changedTouches,
        null,
        initialTouches
      );
      if (touches && touches.pointers) {
        if (touches.pointers.length === 2) {
          callHandler("onPinchEnd", currentTouches, handlers);
        } else {
          callHandler("onPanEnd", currentTouches, handlers);
        }
      }

      if (gesture) {
        callHandler(
          `on${gesture.charAt(0).toUpperCase() + gesture.slice(1)}End`,
          currentTouches,
          handlers
        );
      }
    };

    element?.addEventListener("touchstart", handleTouchStart);
    element?.addEventListener("touchmove", handleTouchMove);
    element?.addEventListener("touchend", handleTouchEnd);
    return () => {
      element?.removeEventListener("touchstart", handleTouchStart);
      element?.removeEventListener("touchmove", handleTouchMove);
      element?.removeEventListener("touchend", handleTouchEnd);
    };
    // run any time
  });
}

export default useGestures;
