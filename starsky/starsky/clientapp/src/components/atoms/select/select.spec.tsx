import { fireEvent, render } from "@testing-library/react";
import React from "react";
import Select from "./select";

describe("SwitchButton", () => {
  it("renders", () => {
    render(<Select selectOptions={[]} />);
  });

  it("trigger change", () => {
    var outputSpy = jest.fn();
    var component = render(
      <Select selectOptions={["Test"]} callback={outputSpy} />
    );

    const selectElement = component.container.querySelector(
      "select"
    ) as HTMLSelectElement;

    fireEvent.change(selectElement, new Event("test"));

    // const changeEvent = createEvent.change(selectElement, {
    //   target: { value: "test" }
    // });

    // await fireEvent(selectElement, changeEvent);

    // component.find("select").simulate("change", { target: { value: "test" } });

    expect(outputSpy).toBeCalled();
    expect(outputSpy).toBeCalledWith("test");
  });

  it("trigger change (no callback)", () => {
    var outputSpy = jest.fn();
    var component = render(<Select selectOptions={[]} />);
    component.find("select").simulate("change", { target: { value: "test" } });

    expect(outputSpy).toBeCalledTimes(0);
  });

  it("find option", () => {
    var component = render(<Select selectOptions={["Test"]} />);

    expect(component.find("option").text()).toBe("Test");
  });

  it("null option", () => {
    var component = render(<Select selectOptions={[]} />);

    expect(component.exists("select")).toBeTruthy();
  });
});
