import { createEvent, fireEvent, render, screen, waitFor } from "@testing-library/react";
import Select from "./select";

describe("SwitchButton", () => {
  it("renders", () => {
    render(<Select selectOptions={[]} />);
  });

  it("trigger change", async () => {
    const outputSpy = jest.fn();
    const component = render(<Select selectOptions={["Test"]} callback={outputSpy} />);

    const selectElement = component.queryByTestId("select") as HTMLElement;

    const changeEvent = createEvent.change(selectElement, {
      value: "Test"
    });

    fireEvent(selectElement, changeEvent);

    await waitFor(() => {
      expect(outputSpy).toHaveBeenCalled();
    });
    await waitFor(() => {
      expect(outputSpy).toHaveBeenCalledWith("Test");
    });
  });

  it("trigger change (no callback)", () => {
    const outputSpy = jest.fn();
    const component = render(<Select selectOptions={[]} />);
    const selectElement = component.queryByTestId("select") as HTMLElement;

    const changeEvent = createEvent.change(selectElement, {
      value: "Test"
    });

    fireEvent(selectElement, changeEvent);

    expect(outputSpy).toHaveBeenCalledTimes(0);
  });

  it("find option", () => {
    const component = render(<Select selectOptions={["Test"]} />);

    const selectElement = screen.queryByTestId("select") as HTMLElement;

    expect(selectElement.querySelector("option")?.innerHTML).toBe("Test");

    component.unmount();
  });

  it("null option", () => {
    const component = render(<Select selectOptions={[]} />);

    const selectElement = screen.queryByTestId("select") as HTMLElement;

    expect(selectElement).toBeTruthy();

    component.unmount();
  });
});
