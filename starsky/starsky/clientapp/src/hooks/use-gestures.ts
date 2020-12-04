import {
  MutableRefObject,
  RefObject,
  useEffect,
  useRef,
  useState
} from "react";

interface IHandlers {
  onPanStart?: Function;
  onPanMove?: Function;
  onSwipeLeft?: Function;
  onSwipeRight?: Function;
  onSwipeUp?: Function;
  onSwipeDown?: Function;
  onPanEnd?: Function;
  onSwipeLeftEnd?: Function;
  onSwipeRightEnd?: Function;
  onSwipeUpEnd?: Function;
  onSwipeDownEnd?: Function;
  onPinchStart?: Function;
  onPinchChanged?: Function;
  onPinchEnd?: Function;
}

export class Pointer {
  public x: number = 0;
  public y: number = 0;

  /**
   * Point element
   * @param {{clientX:number, clientY: number}} touch event touch object
   */
  constructor(touch: any) {
    this.x = touch.clientX;
    this.y = touch.clientY;
  }
}

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

interface ICurrentTouches {
  preventDefault: any;
  stopPropagation?: any;
  pointers?: Pointer[];
  delta: number;
  scale?: number;
  distance: number;
  angleDeg: number;
  deltaX?: number;
  deltaY?: number;
  x?: number;
  y?: number;
}

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

  console.log(pointer.x, pointer.y, touches);

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
export default function useGestures(
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

    const callHandler = (eventName: string, event: ICurrentTouches) => {
      if (
        eventName &&
        (handlers as any)[eventName] &&
        typeof (handlers as any)[eventName] === "function"
      ) {
        (handlers as any)[eventName](event);
      }
    };

    const handleTouchStart = (event: globalThis.TouchEvent) => {
      console.log(event.touches);

      const currentTouches = getCurrentTouches(
        event,
        event.touches,
        null,
        initialTouches
      );
      setTouches(currentTouches);
      initialTouches.current = currentTouches;

      if (event.touches.length === 2) {
        callHandler("onPinchStart", currentTouches);
      } else {
        callHandler("onPanStart", currentTouches);
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
        callHandler("onPinchChanged", currentTouches);
        return;
      }

      console.log(currentTouches.deltaX);
      console.log(currentTouches.deltaY);

      callHandler("onPanMove", currentTouches);

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

      console.log(eventName);

      if (eventName) {
        debounce(
          (eventName: string, touches: ICurrentTouches, theGesture: string) => {
            callHandler(eventName, touches);
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
          callHandler("onPinchEnd", currentTouches);
        } else {
          callHandler("onPanEnd", currentTouches);
        }
      }

      if (gesture) {
        callHandler(
          `on${gesture.charAt(0).toUpperCase() + gesture.slice(1)}End`,
          currentTouches
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
