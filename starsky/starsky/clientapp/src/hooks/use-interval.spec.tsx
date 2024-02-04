import { render } from "@testing-library/react";
import React, { memo } from "react";
import useInterval from "./use-interval";

describe("useInterval", () => {
  interface UseIntervalComponentTestProps {
    callback: Function;
    timer: number;
  }

  const UseIntervalComponentTest: React.FunctionComponent<UseIntervalComponentTestProps> = memo(
    (props) => {
      useInterval(props.callback, props.timer);
      return <></>;
    }
  );

  it("check if is called once", (done) => {
    function callback() {
      done();
    }
    render(<UseIntervalComponentTest timer={0} callback={callback}></UseIntervalComponentTest>);
  });

  it("check if setInterval is called", () => {
    const clearIntervalSpy = jest.spyOn(window, "setInterval").mockImplementationOnce(() => {
      return {} as any;
    });

    const component = render(<UseIntervalComponentTest timer={10} callback={jest.fn()} />);
    component.unmount();

    expect(clearIntervalSpy).toHaveBeenCalled();
  });
});
