import { render } from "@testing-library/react";
import React, { MutableRefObject, useRef } from "react";
import { mockUnobserve, triggerIntersection } from "./___tests___/intersection-observer-mock";
import useIntersection, {
  IntersectionOptions,
  newIntersectionObserver
} from "./use-intersection-observer";

describe("useIntersection", () => {
  const IntersectionComponentTest = () => {
    const target = useRef<HTMLDivElement>(null);
    render(<div ref={target} />);
    useIntersection(target);
    return null;
  };

  it("call api", () => {
    const focus = jest.fn();
    const useRefSpy = jest
      .spyOn(React, "useRef")
      .mockReset()
      .mockReturnValueOnce({ current: { focus } })
      .mockReturnValueOnce({ current: { focus } })
      .mockReturnValueOnce({ current: { focus } })
      .mockReturnValueOnce({ current: { focus } });

    render(<IntersectionComponentTest></IntersectionComponentTest>);
    expect(useRefSpy).toHaveBeenCalled();
  });

  const NewIntersectionComponentTest = () => {
    const target = useRef<HTMLDivElement>(null);
    render(<div ref={target} />);
    const tagRef = { current: { scrollHeight: 100, clientHeight: 200 } };
    newIntersectionObserver(
      target,
      jest.fn(),
      true,
      tagRef as unknown as MutableRefObject<IntersectionOptions>
    );
    return null;
  };

  it("newIntersectionObserver is not failing", () => {
    render(<NewIntersectionComponentTest />);
    // there is no assert/check
  });

  describe("newIntersectionObserver", () => {
    it("triggers the callback and sets intersecting state", () => {
      const ref = { current: document.createElement("div") } as React.RefObject<Element>;
      const setIntersecting = jest.fn();
      const callback = jest.fn();
      const optsRef = {
        current: {
          root: null,
          rootMargin: "0px",
          threshold: 0.1
        }
      } as unknown as React.MutableRefObject<IntersectionOptions>;

      const observer = newIntersectionObserver(ref, setIntersecting, true, optsRef, callback);
      observer.observe(ref.current!);

      expect(setIntersecting).toHaveBeenCalledTimes(0);

      // simulate intersection
      triggerIntersection([
        {
          isIntersecting: true,
          target: ref.current!
        }
      ]);

      expect(setIntersecting).toHaveBeenCalledWith(true);
      expect(callback).toHaveBeenCalled();
      expect(mockUnobserve).toHaveBeenCalledWith(ref.current);
    });
  });
});
