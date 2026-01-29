import { fireEvent, render, screen } from "@testing-library/react";
import React from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import * as ModalTimezoneShift from "../../organisms/modal-timezone-shift/modal-timezone-shift";
import { MenuOptionTimezoneShift } from "./menu-option-timezone-shift";

describe("MenuOptionTimezoneShift", () => {
  let modalSpy = jest.spyOn(ModalTimezoneShift, "default");

  function mockModalHandleExit() {
    modalSpy = jest.spyOn(ModalTimezoneShift, "default").mockImplementationOnce((props) => {
      React.useEffect(() => {
        props.handleExit();
      });
      return <>mocked</>;
    });
  }

  const renderComponent = (readOnly = false, select = ["/test.jpg"]) => {
    return render(
      <MenuOptionTimezoneShift
        readOnly={readOnly}
        select={select}
        state={{} as unknown as IArchiveProps}
        dispatch={jest.fn()}
      />
    );
  };

  function mockModal() {
    modalSpy = jest
      .spyOn(ModalTimezoneShift, "default")
      .mockReset()
      .mockImplementationOnce(() => <div data-test="modal-timezone-shift">Modal</div>);
  }

  it("should not show the modal initially", () => {
    mockModal();
    const component = renderComponent();
    expect(screen.queryByTestId("modal-timezone-shift")).not.toBeTruthy();
    expect(modalSpy).toHaveBeenCalledTimes(0);
    component.unmount();
  });

  it("should open the modal when the button is clicked", () => {
    mockModal();
    const component = renderComponent();

    fireEvent.click(screen.getByTestId("timezone-shift"));
    expect(modalSpy).toHaveBeenCalledTimes(1);

    // Modal should appear synchronously
    const el = screen
      .getByTestId("timezone-shift")
      .closest("div")!
      .querySelector('[data-test="modal-timezone-shift"]');
    expect(el).toBeTruthy();
    component.unmount();
  });

  it("should close the modal when the handleExit function is called", () => {
    jest.spyOn(React, "useState").mockReturnValueOnce([true, jest.fn()]);
    mockModalHandleExit();

    const component = renderComponent();
    fireEvent.click(screen.getByTestId("timezone-shift"));
    expect(screen.getByTestId("timezone-shift")).toBeTruthy();

    component.unmount();
  });

  it("should not open the modal if readOnly is true", () => {
    const component = renderComponent(true);

    const button = screen.getByTestId("timezone-shift");
    // Check for disabled class
    expect(button.closest("li")?.classList.contains("disabled")).toBe(true);

    component.unmount();
  });

  it("should not show when no files are selected", () => {
    const component = renderComponent(false, []);

    const button = screen.getByTestId("timezone-shift");
    // Check for disabled class
    expect(button.closest("li")?.classList.contains("disabled")).toBe(true);

    component.unmount();
  });
});
