import { render } from "@testing-library/react";
import React, { MutableRefObject, useRef } from "react";
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
});
