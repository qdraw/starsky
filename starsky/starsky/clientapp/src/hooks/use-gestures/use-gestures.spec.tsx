import { act, createEvent, fireEvent, render } from "@testing-library/react";
import React, { useRef, useState } from "react";
import { mountReactHook } from "../___tests___/test-hook";
import * as callHandler from "./call-handler";
import * as debounce from "./debounce";
import * as getCurrentTouchesAll from "./get-current-touches";
import { getCurrentTouches } from "./get-current-touches";
import { Pointer } from "./pointer";
import { getAngleDeg, getDistance, useGestures } from "./use-gestures";

function Rotate() {
  const [imageRotation, setImageRotation] = useState(0);
  const image = useRef<HTMLImageElement>(null);

  useGestures(image, {
    onPanMove: (event: any) => {
      setImageRotation(event.angleDeg);
    }
  });

  return (
    <img
      ref={image}
      alt="React Logo"
      className="logo"
      style={{ transform: "rotate(" + imageRotation + "deg)" }}
    />
  );
}

describe("useGestures", () => {
  describe("Pointer", () => {
    it("check output of pointer", () => {
      const p = new Pointer({ clientX: 10, clientY: 11 });
      expect(p.x).toBe(10);
      expect(p.y).toBe(11);
    });
  });

  describe("getDistance", () => {
    it("undefined", () => {
      const p = getDistance(new Pointer({ clientX: 0, clientY: 0 }), {
        x: undefined,
        y: undefined
      } as any);
      expect(p).toBe(0);
    });

    it("check output of getDistance", () => {
      const p = getDistance(
        new Pointer({ clientX: 10, clientY: 11 }),
        new Pointer({ clientX: 10, clientY: 11 })
      );
      expect(p).toBe(0);
    });

    it("check output of getDistance and two length", () => {
      const p = getDistance(
        new Pointer({ clientX: 10, clientY: 11 }),
        new Pointer({ clientX: 12, clientY: 11 })
      );
      expect(p).toBe(2);
    });
  });

  describe("getAngleDeg", () => {
    it("undefined", () => {
      const p = getAngleDeg(new Pointer({ clientX: 0, clientY: 0 }), {
        x: undefined,
        y: undefined
      } as any);
      expect(p).toBe(0);
    });

    it("check output of getDistance", () => {
      const p = getAngleDeg(
        new Pointer({ clientX: 10, clientY: 11 }),
        new Pointer({ clientX: 10, clientY: 11 })
      );
      expect(p).toBe(0);
    });

    it("check output of getDistance and two length", () => {
      const p = getAngleDeg(
        new Pointer({ clientX: 10, clientY: 11 }),
        new Pointer({ clientX: 12, clientY: 11 })
      );
      expect(p).toBe(180);
    });
  });

  describe("getCurrentTouches", () => {
    it("single touch event", () => {
      const sourceEvent = {
        originalEvent: jest.fn(),
        stopPropagation: jest.fn()
      };
      const newTouches = [
        {
          clientX: 20,
          clientY: 0
        }
      ];

      const prevTouch = {
        clientX: 10,
        clientY: 0
      };

      const t = { current: { x: 1, y: 1 } };

      const result = getCurrentTouches(
        sourceEvent as any,
        newTouches as any,
        prevTouch as any,
        t as any
      );

      expect(result.delta).toBe(20);
      expect(result.x).toBe(20);
    });

    it("dual touch event", () => {
      const sourceEvent = {
        originalEvent: jest.fn(),
        stopPropagation: jest.fn()
      };
      const newTouches = [
        {
          clientX: 20,
          clientY: 0
        },
        {
          clientX: 40,
          clientY: 0
        }
      ];

      const prevTouch = {
        clientX: 10,
        clientY: 0
      };

      const t = { current: { x: 1, y: 1 } };

      const result = getCurrentTouches(
        sourceEvent as any,
        newTouches as any,
        prevTouch as any,
        t as any
      );

      expect(result.angleDeg).toBe(180);
      expect(result.distance).toBe(20);
    });
  });

  describe("useGestures", () => {
    const exampleSingleTouches = {
      touches: [
        {
          clientX: 10,
          clientY: 0
        } as any
      ]
    };

    const exampleDoubleTouches = {
      touches: [
        {
          clientX: 10,
          clientY: 0
        } as any,
        {
          clientX: 10,
          clientY: 0
        } as any
      ]
    };

    it("check if is called once", () => {
      jest.useFakeTimers();
      const component = render(<Rotate />);

      const image = component.container.querySelector("img") as HTMLImageElement;
      const touchmoveEvent = createEvent.wheel(image, exampleSingleTouches);

      fireEvent(image, touchmoveEvent);

      jest.advanceTimersByTime(201);

      // this does nothing

      jest.useRealTimers();
    });

    it("touchstart single onPanStart", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchstart", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPanStart", expect.anything(), undefined);
      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchstart double onPinchStart", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchstart", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchStart", expect.anything(), undefined);
      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove single onPanMove", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPanMove", expect.anything(), undefined);
      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove deltaX/Y undefined", () => {
      jest
        .spyOn(getCurrentTouchesAll, "getCurrentTouches")
        .mockReset()
        .mockImplementationOnce(() => {
          return {
            deltaX: undefined,
            deltaY: undefined
          } as any;
        });

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalledTimes(0);

      const component = hook.componentMount as any;
      component.unmount();
      debounceSpy.mockReset();
    });

    it("touchmove large delta swipe right should call deBounce", () => {
      jest.spyOn(getCurrentTouchesAll, "getCurrentTouches").mockImplementationOnce(() => {
        return {
          deltaX: 30,
          deltaY: 0
        } as any;
      });

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeRight", {}, "swipeRight");

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove large delta swipeLeft should call deBounce", () => {
      jest
        .spyOn(getCurrentTouchesAll, "getCurrentTouches")
        .mockReset()
        .mockImplementationOnce(() => {
          return {
            deltaX: -30,
            deltaY: 0
          } as any;
        });
      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeLeft", {}, "swipeLeft");

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove large delta swipeDown should call deBounce", () => {
      jest
        .spyOn(getCurrentTouchesAll, "getCurrentTouches")
        .mockReset()
        .mockImplementationOnce(() => {
          return {
            deltaX: 0,
            deltaY: 30
          } as any;
        });
      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeDown", {}, "swipeDown");

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove large delta onSwipeUp should call deBounce", () => {
      jest
        .spyOn(getCurrentTouchesAll, "getCurrentTouches")
        .mockReset()
        .mockImplementationOnce(() => {
          return {
            deltaX: 0,
            deltaY: -30
          } as any;
        });

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeUp", {}, "swipeUp");

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchmove double", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchChanged", undefined, undefined);

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchend single onPinchEnd", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {});

      jest
        .spyOn(React, "useState")
        .mockReset()
        .mockImplementationOnce(() => [
          {
            pointers: ["", "1"],
            charAt: () => {
              return {
                toUpperCase: jest.fn()
              };
            },
            slice: jest.fn()
          },
          jest.fn()
        ])
        .mockImplementationOnce(() => [
          {
            pointers: ["", "1"],
            charAt: () => {
              return {
                toUpperCase: jest.fn()
              };
            },
            slice: jest.fn()
          },
          jest.fn()
        ]);

      const touchEndEvent = new TouchEvent("touchend", exampleSingleTouches);

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      act(() => {
        demoElement.dispatchEvent(touchEndEvent);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchEnd", undefined, undefined);

      const component = hook.componentMount as any;
      component.unmount();
    });

    it("touchend double onPinchEnd", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {})
        .mockImplementationOnce(() => {});

      jest
        .spyOn(React, "useState")
        .mockReset()
        .mockImplementationOnce(() => [{ pointers: ["a"] }, jest.fn()])
        .mockImplementationOnce(() => ["gesture1", jest.fn()]);

      const touchEndEvent = new TouchEvent("touchend", exampleDoubleTouches);

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures, [{ current: demoElement }]);

      act(() => {
        demoElement.dispatchEvent(touchEndEvent);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenNthCalledWith(1, "onPanEnd", undefined, undefined);
      expect(callHandlerSpy).toHaveBeenNthCalledWith(2, "onGesture1End", undefined, undefined);
      const component = hook.componentMount as any;
      component.unmount();
    });
  });
});
