import { act } from "@testing-library/react";
import { mount, ReactWrapper } from "enzyme";
import React, { useRef, useState } from "react";
import { mountReactHook } from "../___tests___/test-hook";
import * as callHandler from "./call-handler";
import * as getCurrentTouchesAll from "./get-current-touches";
import { getCurrentTouches } from "./get-current-touches";
import * as PointerAll from "./pointer";
import { Pointer } from "./pointer";
import { getAngleDeg, getDistance, useGestures } from "./use-gestures";

function Rotate() {
  const [imageRotation, setImageRotation] = useState(0);
  const image = useRef<HTMLImageElement>(null);

  useGestures(image, {
    onPanMove: (event: any) => {
      setImageRotation(event.angleDeg);
    },
    onPanEnd: (event: any) => {
      setImageRotation(1);
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

    it("check output of getDistance and two lenght", () => {
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

    it("check output of getDistance and two lenght", () => {
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

      var result = getCurrentTouches(
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

      var result = getCurrentTouches(
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
    xit("check if is called once", () => {
      var test = mount(<Rotate />);
      console.log(test);
    });

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

    it("touchstart single onPanStart", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      var hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchstart", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toBeCalled();
      expect(callHandlerSpy).toBeCalledWith(
        "onPanStart",
        expect.anything(),
        undefined
      );
      const component = (hook.componentMount as any) as ReactWrapper;
      component.unmount();
    });

    it("touchstart double onPinchStart", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      var hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchstart", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toBeCalled();
      expect(callHandlerSpy).toBeCalledWith(
        "onPinchStart",
        expect.anything(),
        undefined
      );
      const component = (hook.componentMount as any) as ReactWrapper;
      component.unmount();
    });

    it("touchmove single onPanMove", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      var hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleSingleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toBeCalled();
      expect(callHandlerSpy).toBeCalledWith(
        "onPanMove",
        expect.anything(),
        undefined
      );
      const component = (hook.componentMount as any) as ReactWrapper;
      component.unmount();
    });

    it("touchmove double", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockImplementationOnce(() => {});

      const demoElement = document.createElement("div");

      var hook = mountReactHook(useGestures, [{ current: demoElement }]);

      const event = new TouchEvent("touchmove", exampleDoubleTouches);

      act(() => {
        demoElement.dispatchEvent(event);
      });

      expect(callHandlerSpy).toBeCalled();
      expect(callHandlerSpy).toBeCalledWith(
        "onPinchChanged",
        expect.anything(),
        undefined
      );

      const component = (hook.componentMount as any) as ReactWrapper;
      component.unmount();
    });

    it("touchend", () => {
      const callHandlerSpy = jest
        .spyOn(callHandler, "callHandler")
        .mockImplementationOnce(() => {});

      const touchEndEvent = new TouchEvent("touchend", exampleSingleTouches);
      const touchStartEvent = new TouchEvent(
        "touchstart",
        exampleSingleTouches
      );

      jest
        .spyOn(getCurrentTouchesAll, "getCurrentTouches")
        .mockImplementationOnce(() => {
          return {
            deltaX: 1,
            deltaY: 2
          } as any;
        });

      jest.spyOn(PointerAll, "Pointer").mockImplementationOnce(() => {
        return {
          x: 1,
          y: 1
        };
      });

      const demoElement = document.createElement("div");

      var hook = mountReactHook(useGestures, [{ current: demoElement }]);

      act(() => {
        demoElement.dispatchEvent(touchStartEvent);
        demoElement.dispatchEvent(touchEndEvent);
      });

      expect(callHandlerSpy).toBeCalled();
      expect(callHandlerSpy).toBeCalledTimes(1);

      const component = (hook.componentMount as any) as ReactWrapper;
      component.unmount();
    });
  });
});
