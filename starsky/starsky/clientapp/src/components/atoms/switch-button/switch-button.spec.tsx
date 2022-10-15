import { act, fireEvent, render } from "@testing-library/react";
import React from "react";
import SwitchButton from "./switch-button";

describe("SwitchButton", () => {
  it("renders", () => {
    const toggle = jest.fn();
    render(
      <SwitchButton onToggle={toggle} leftLabel={"on"} rightLabel={"off"} />
    );
  });

  it("renders (disabled:state)", () => {
    const toggle = jest.fn();
    const wrapper = render(
      <SwitchButton
        isEnabled={false}
        onToggle={toggle}
        leftLabel={"on"}
        rightLabel={"off"}
      />
    );

    const switchButton = wrapper.queryByTestId(
      "switch-button-left"
    ) as HTMLInputElement;

    expect(switchButton.disabled).toBeTruthy();
  });

  it("test if element triggers onToggle when changed (default)", () => {
    const toggle = jest.fn();
    const wrapper = render(
      <SwitchButton
        isOn={true}
        onToggle={toggle}
        leftLabel={"on label"}
        rightLabel={"off label"}
      />
    );

    const switchButtonLeft = wrapper.queryByTestId(
      "switch-button-left"
    ) as HTMLInputElement;

    act(() => {
      fireEvent.click(switchButtonLeft);
    });

    expect(toggle).toBeCalledTimes(1);
  });

  it("test if element triggers onToggle when changed (negative)", () => {
    const toggle = jest.fn();
    const wrapper = render(
      <SwitchButton
        isOn={false}
        onToggle={toggle}
        leftLabel={"on"}
        rightLabel={"off"}
      />
    );

    const switchButtonRight = wrapper.queryByTestId(
      "switch-button-right"
    ) as HTMLInputElement;

    act(() => {
      fireEvent.click(switchButtonRight);
    });

    expect(toggle).toBeCalled();
  });
});
