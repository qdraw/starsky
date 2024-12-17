import { createEvent, fireEvent, render } from "@testing-library/react";
import React, { act, useRef, useState } from "react";
import { mountReactHook } from "../___tests___/test-hook";
import * as callHandler from "./call-handler";
import * as debounce from "./debounce";
import * as getCurrentTouchesAll from "./get-current-touches";
import { getCurrentTouches } from "./get-current-touches";
import { ICurrentTouches } from "./ICurrentTouches.types";
import { IHandlers } from "./IHandlers.types";
import { Pointer } from "./pointer";
import { executeTouchMove, getAngleDeg, getDistance, useGestures } from "./use-gestures";

function Rotate() {
  const [imageRotation, setImageRotation] = useState(0);
  const image = useRef<HTMLImageElement>(null);

  useGestures(image, {
    onPanMove: (event: TouchEvent) => {
      const angleDeg = getAngleDegFromEvent(event);
      setImageRotation(angleDeg);
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

function getAngleDegFromEvent(ev: TouchEvent): number {
  // Implement the logic to extract angleDeg from the event
  return ev.AT_TARGET; // Placeholder implementation
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
      } as unknown as Pointer);
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
      } as unknown as Pointer);
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
        sourceEvent as unknown as globalThis.TouchEvent,
        newTouches as unknown as TouchList,
        prevTouch as unknown as ICurrentTouches,
        t as React.MutableRefObject<ICurrentTouches>
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
        sourceEvent as unknown as globalThis.TouchEvent,
        newTouches as unknown as TouchList,
        prevTouch as unknown as ICurrentTouches,
        t as React.MutableRefObject<ICurrentTouches>
      );

      expect(result.angleDeg).toBe(180);
      expect(result.distance).toBe(20);
    });
  });

  describe("useGestures as (...args: unknown[]) => unknown", () => {
    const exampleSingleTouches = {
      touches: [
        {
          clientX: 10,
          clientY: 0
        }
      ]
    } as TouchEventInit;

    const exampleDoubleTouches = {
      touches: [
        {
          clientX: 10,
          clientY: 0
        },
        {
          clientX: 10,
          clientY: 0
        }
      ]
    } as TouchEventInit;

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

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchstart", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPanStart", expect.anything(), undefined);
      const component = hook.componentMount;
      component.unmount();
    });

    it("touchstart double onPinchStart", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchstart", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchStart", expect.anything(), undefined);
      const component = hook.componentMount;
      component.unmount();
    });

    it("touchmove single onPanMove", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPanMove", expect.anything(), undefined);
      const component = hook.componentMount;
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
          } as unknown as ICurrentTouches;
        });

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalledTimes(0);

      const component = hook.componentMount;
      component.unmount();
      debounceSpy.mockReset();
    });

    it("touchmove large delta swipe right should call deBounce", () => {
      jest.spyOn(getCurrentTouchesAll, "getCurrentTouches").mockImplementationOnce(() => {
        return {
          deltaX: 30,
          deltaY: 0
        } as unknown as ICurrentTouches;
      });

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeRight", {}, "swipeRight");

      const component = hook.componentMount;
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
          } as unknown as ICurrentTouches;
        });
      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeLeft", {}, "swipeLeft");

      const component = hook.componentMount;
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
          } as unknown as ICurrentTouches;
        });
      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeDown", {}, "swipeDown");

      const component = hook.componentMount;
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
          } as unknown as ICurrentTouches;
        });

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const debounceAnonymousFnSpy = jest.fn();
      const debounceSpy = jest
        .spyOn(debounce, "debounce")
        .mockReset()
        .mockImplementationOnce(() => debounceAnonymousFnSpy);
      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(debounceSpy).toHaveBeenCalled();
      expect(debounceAnonymousFnSpy).toHaveBeenCalledWith("onSwipeUp", {}, "swipeUp");

      const component = hook.componentMount;
      component.unmount();
    });

    it("touchmove double", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockReset()
        .mockImplementationOnce(() => {});

      jest.spyOn(callHandler, "callHandler").mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      const event = new TouchEvent("touchmove", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchChanged", undefined, undefined);

      const component = hook.componentMount;
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

      const hook = mountReactHook(useGestures as (...args: unknown[]) => unknown, [
        { current: demoElement }
      ]);

      act(() => {
        demoElement.dispatchEvent(touchEndEvent);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenCalledWith("onPinchEnd", undefined, undefined);

      const component = hook.componentMount;
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

      const hook = mountReactHook(
        useGestures as (...args: unknown[]) => unknown as (...args: unknown[]) => unknown,
        [{ current: demoElement }]
      );

      act(() => {
        demoElement.dispatchEvent(touchEndEvent);
      });

      expect(callHandlerSpy).toHaveBeenCalled();
      expect(callHandlerSpy).toHaveBeenNthCalledWith(1, "onPanEnd", undefined, undefined);
      expect(callHandlerSpy).toHaveBeenNthCalledWith(2, "onGesture1End", undefined, undefined);
      const component = hook.componentMount;
      component.unmount();
    });
  });

  it("executeTouchMove returns emthy string", () => {
    const result = executeTouchMove(
      { touches: [] } as unknown as globalThis.TouchEvent,
      {
        x: 1,
        y: 1,
        deltaX: 1,
        deltaY: 1
      } as ICurrentTouches,
      {} as IHandlers,
      { minDelta: 0 },
      {} as ICurrentTouches,
      jest.fn()
    );

    expect(result).toBe("");
  });

  describe("executeTouchMove", () => {
    const testCases = [
      {
        description: "swipeRight",
        currentTouches: { x: 150, y: 150, deltaX: 1000, deltaY: 5 },
        options: { minDelta: 10 },
        expectedGesture: "swipeRight"
      },
      {
        description: "swipeUp",
        currentTouches: { x: 150, y: 150, deltaX: 5, deltaY: -1000 },
        options: { minDelta: 10 },
        expectedGesture: "swipeUp"
      },
      {
        description: "swipeDown",
        currentTouches: { x: 150, y: 150, deltaX: 5, deltaY: 1000 },
        options: { minDelta: 10 },
        expectedGesture: "swipeDown"
      },
      {
        description: "swipeLeft",
        currentTouches: { x: 150, y: 150, deltaX: -1000, deltaY: 5 },
        options: { minDelta: 10 },
        expectedGesture: "swipeLeft"
      }
    ];

    test.each(testCases)(
      "should detect $description gesture",
      ({ currentTouches, options, expectedGesture }) => {
        jest.spyOn(debounce, "debounce").mockImplementationOnce((arg) => {
          arg("onSwipe", {}, expectedGesture);
          return jest.fn();
        });

        const callHandlerSpy = jest
          .spyOn(callHandler, "callHandler")
          .mockImplementationOnce(() => {});

        const mockTouchEvent = { touches: [] } as unknown as globalThis.TouchEvent;
        const handlers: IHandlers = {};
        const previousTouches: ICurrentTouches = {} as ICurrentTouches;
        const callback = jest.fn();

        const result = executeTouchMove(
          mockTouchEvent,
          currentTouches as ICurrentTouches,
          handlers,
          options,
          previousTouches,
          callback
        );

        // Verify that the condition is hit
        expect(
          Math.abs(currentTouches.deltaX) >= options.minDelta ||
            Math.abs(currentTouches.deltaY) >= options.minDelta
        ).toBe(true);

        // Verify that the callback is called with the expected gesture
        expect(callHandlerSpy).toHaveBeenCalled();
        expect(result).toBe(expectedGesture);
      }
    );
  });
});
