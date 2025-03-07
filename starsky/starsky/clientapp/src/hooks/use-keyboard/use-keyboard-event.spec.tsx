import { render } from "@testing-library/react";
import React, { memo } from "react";
import useKeyboardEvent from "./use-keyboard-event";

describe("useKeyboardEvent", () => {
  interface UseKeyboardEventComponentTestProps {
    regex: RegExp;
    callback: (arg0: KeyboardEvent) => void;
    dependencies: React.DependencyList;
  }

  const UseKeyboardEventComponentTest: React.FunctionComponent<UseKeyboardEventComponentTestProps> =
    memo((props) => {
      useKeyboardEvent(props.regex, props.callback, props.dependencies);
      return null;
    });

  it("check if is called once", () => {
    const callback = jest.fn();
    render(
      <UseKeyboardEventComponentTest
        dependencies={[]}
        regex={new RegExp("q")}
        callback={callback}
      ></UseKeyboardEventComponentTest>
    );

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "q",
      shiftKey: true
    });
    window.dispatchEvent(event);

    expect(callback).toHaveBeenCalled();
    expect(callback).toHaveBeenCalledTimes(1);
  });

  it("to be not called input z => check for q", () => {
    const callback = jest.fn();
    render(
      <UseKeyboardEventComponentTest
        dependencies={[]}
        regex={new RegExp("q")}
        callback={callback}
      />
    );

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "z",
      shiftKey: true
    });
    window.dispatchEvent(event);
    expect(callback).toHaveBeenCalledTimes(0);
  });
});
