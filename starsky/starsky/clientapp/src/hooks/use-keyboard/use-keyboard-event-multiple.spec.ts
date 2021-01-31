import React from "react";
import { mountReactHook } from "../___tests___/test-hook";
import useKeyboardEventMultiple from "./use-keyboard-event-multiple";

describe("useKeyboardEventMultiple", () => {
  it("should add to set list when keyDown", () => {
    const useStateSetter = jest.fn();
    const useStateSpy = jest
      .spyOn(React, "useState")
      .mockImplementationOnce(() => [
        new Set(new Array<string>()),
        useStateSetter
      ]);
    const test = mountReactHook(useKeyboardEventMultiple, []);

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "q",
      shiftKey: true
    });
    window.dispatchEvent(event);

    expect(useStateSpy).toBeCalled();
    expect(useStateSetter).toBeCalledWith(new Set(["q"]));

    test.componentMount.unmount();
  });

  it("should remove from set list when keyUp", () => {
    const useStateSetter = jest.fn();
    const useStateSpy = jest
      .spyOn(React, "useState")
      .mockImplementationOnce(() => [new Set(new Set(["q"])), useStateSetter]);
    const test = mountReactHook(useKeyboardEventMultiple, []);

    const event = new KeyboardEvent("keyup", {
      bubbles: true,
      cancelable: true,
      key: "q",
      shiftKey: true
    });
    window.dispatchEvent(event);

    expect(useStateSpy).toBeCalled();
    expect(useStateSetter).toBeCalledWith(new Set());

    test.componentMount.unmount();
  });

  // xit("to be not called input z => check for q", () => {
  //   var callback = jest.fn();
  //   mount(
  //     <UseKeyboardEventComponentTest
  //       dependencies={[]}
  //       regex={new RegExp("q")}
  //       callback={callback}
  //     />
  //   );

  //   var event = new KeyboardEvent("keydown", {
  //     bubbles: true,
  //     cancelable: true,
  //     key: "z",
  //     shiftKey: true
  //   });
  //   window.dispatchEvent(event);
  //   expect(callback).toBeCalledTimes(0);
  // });
});
