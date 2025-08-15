import { render } from "@testing-library/react";
import React, { memo } from "react";
import { MemoryRouter } from "react-router-dom";
import useInterval from "./use-interval";

describe("useInterval", () => {
  interface UseIntervalComponentTestProps {
    callback: () => void;
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
    render(
      <MemoryRouter>
        <UseIntervalComponentTest timer={0} callback={callback}></UseIntervalComponentTest>
      </MemoryRouter>
    );
  });

  it("check if setInterval is called", () => {
    const clearIntervalSpy = jest.spyOn(window, "setInterval").mockImplementationOnce(() => {
      return {} as NodeJS.Timeout;
    });

    const component = render(<UseIntervalComponentTest timer={10} callback={jest.fn()} />);
    component.unmount();

    expect(clearIntervalSpy).toHaveBeenCalled();
  });
});
