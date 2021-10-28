import {
  createEvent,
  fireEvent,
  render,
  waitFor
} from "@testing-library/react";
import React from "react";
import Select from "./select";

describe("SwitchButton", () => {
  it("renders", () => {
    render(<Select selectOptions={[]} />);
  });

  it("trigger change", async () => {
    var outputSpy = jest.fn();
    var component = render(
      <Select selectOptions={["Test"]} callback={outputSpy} />
    );

    const selectElement = component.queryByTestId("select") as HTMLElement;

    const changeEvent = createEvent.change(selectElement, {
      value: "Test"
    });

    fireEvent(selectElement, changeEvent);

    await waitFor(() => {
      expect(outputSpy).toBeCalled();
      expect(outputSpy).toBeCalledWith("Test");
    });
  });

  it("trigger change (no callback)", () => {
    var outputSpy = jest.fn();
    var component = render(<Select selectOptions={[]} />);
    const selectElement = component.queryByTestId("select") as HTMLElement;

    const changeEvent = createEvent.change(selectElement, {
      value: "Test"
    });

    fireEvent(selectElement, changeEvent);

    expect(outputSpy).toBeCalledTimes(0);
  });

  it("find option", () => {
    var component = render(<Select selectOptions={["Test"]} />);

    const selectElement = component.queryByTestId("select") as HTMLElement;

    expect(selectElement.querySelector("option")?.innerHTML).toBe("Test");
  });

  it("null option", () => {
    var component = render(<Select selectOptions={[]} />);

    const selectElement = component.queryByTestId("select") as HTMLElement;

    expect(selectElement).toBeTruthy();
  });
});
