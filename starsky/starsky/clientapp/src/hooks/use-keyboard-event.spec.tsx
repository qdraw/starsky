import { mount } from 'enzyme';
import React, { memo } from 'react';
import useKeyboardEvent from './use-keyboard-event';

describe("useKeyboardEvent", () => {

  interface UseKeyboardEventComponentTestProps {
    regex: RegExp,
    callback: Function,
    dependencies: any[]
  }

  const UseKeyboardEventComponentTest: React.FunctionComponent<UseKeyboardEventComponentTestProps> = memo((props) => {
    useKeyboardEvent(props.regex, props.callback, props.dependencies);
    return null;
  });

  it("check if is called once", () => {
    var callback = jest.fn()
    mount(<UseKeyboardEventComponentTest dependencies={[]} regex={new RegExp("q")} callback={callback}/>);

    var event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "q",
      shiftKey: true,
    });
    window.dispatchEvent(event);

    expect(callback).toBeCalled();
    expect(callback).toBeCalledTimes(1);
  });

  it("to be not called input z => check for q", () => {
    var callback = jest.fn()
    mount(<UseKeyboardEventComponentTest dependencies={[]} regex={new RegExp("q")} callback={callback}/>);

    var event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "z",
      shiftKey: true,
    });
    window.dispatchEvent(event);
    expect(callback).toBeCalledTimes(0);
  });

});