import { createEvent, fireEvent, render } from "@testing-library/react";
import React from "react";
import SwitchButton from "./switch-button";

describe("SwitchButton", () => {
  it("renders", () => {
    var toggle = jest.fn();
    render(
      <SwitchButton onToggle={toggle} leftLabel={"on"} rightLabel={"off"} />
    );
  });

  it("renders (disabled:state)", () => {
    var toggle = jest.fn();
    var wrapper = render(
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
    var toggle = jest.fn();
    var wrapper = render(
      <SwitchButton
        isOn={true}
        onToggle={toggle}
        leftLabel={"on"}
        rightLabel={"off"}
      />
    );

    const switchButton = wrapper.queryByTestId(
      "switch-button-right"
    ) as HTMLInputElement;

    const changeEvent = createEvent.change(switchButton, {
      value: "Test"
    });

    fireEvent(switchButton, changeEvent);

    // var name = wrapper.find('[name="switchToggle"]');
    // name.last().simulate("change");
    expect(toggle).toBeCalled();
    expect(name.last().props().disabled).toBeFalsy();
    expect(name.last().props().checked).toBeTruthy();
  });

  it("test if element triggers onToggle when changed (negative)", () => {
    var toggle = jest.fn();
    var wrapper = render(
      <SwitchButton
        isOn={false}
        onToggle={toggle}
        leftLabel={"on"}
        rightLabel={"off"}
      />
    );
    var name = wrapper.find('[name="switchToggle"]');
    name.first().simulate("change");
    expect(toggle).toBeCalled();
    expect(name.last().props().disabled).toBeFalsy();
    expect(name.first().props().checked).toBeTruthy();
  });
});
