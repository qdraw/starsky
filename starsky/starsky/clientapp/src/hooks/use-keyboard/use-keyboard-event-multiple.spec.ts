import { mountReactHook } from "../___tests___/test-hook";
import useKeyboardEventMultiple from "./use-keyboard-event-multiple";

describe("useKeyboardEventMultiple", () => {
  it("check if is called once", () => {
    const callback = jest.fn();
    // mount(
    //   <UseKeyboardEventMultipleComponentTest></UseKeyboardEventMultipleComponentTest>
    // );

    const test = mountReactHook(useKeyboardEventMultiple, []);

    console.log(test);

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "q",
      shiftKey: true
    });
    window.dispatchEvent(event);

    expect(callback).toBeCalled();
    expect(callback).toBeCalledTimes(1);
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
