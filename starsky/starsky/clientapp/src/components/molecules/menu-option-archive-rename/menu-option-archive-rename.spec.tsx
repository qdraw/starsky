import { fireEvent, render, screen } from "@testing-library/react";
import React from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import * as ModalArchiveRename from "../../organisms/modal-archive-rename/modal-archive-rename";
import { MenuOptionArchiveRename } from "./menu-option-archive-rename";

describe("MenuOptionArchiveRename", () => {
  const mockDispatch = jest.fn();
  const mockState = { fileIndexItem: {} } as unknown as IArchiveProps;
  let modalSpy = jest.spyOn(ModalArchiveRename, "default");

  function mockModalHandleExit() {
    modalSpy.mockReset();
    modalSpy = jest.spyOn(ModalArchiveRename, "default").mockImplementation((props) => {
      props.handleExit();
      return <h1 data-test="modal-archive-mkdir">test</h1>;
    });
  }

  function mockModal() {
    modalSpy.mockReset();
    modalSpy = jest.spyOn(ModalArchiveRename, "default").mockImplementation(() => {
      return <h1 data-test="modal-archive-rename">test</h1>;
    });
  }

  const renderComponent = (readOnly = false) => {
    return render(
      <MenuOptionArchiveRename readOnly={readOnly} state={mockState} dispatch={mockDispatch} />
    );
  };

  it("should not show the modal initially", () => {
    mockModal();
    const component = renderComponent();
    expect(screen.queryByTestId("modal-archive-rename")).not.toBeTruthy();
    expect(modalSpy).toHaveBeenCalledTimes(0);
    component.unmount();
  });

  it("should open the modal when the button is clicked", () => {
    mockModal();
    const component = renderComponent();

    fireEvent.click(screen.getByTestId("rename"));
    expect(modalSpy).toHaveBeenCalledTimes(1);

    expect(screen.getByTestId("modal-archive-rename")).toBeTruthy();
    component.unmount();
  });

  it("should close the modal when the handleExit function is called", () => {
    const stateSpy = jest.spyOn(React, "useState").mockReturnValueOnce([true, jest.fn()]);
    mockModalHandleExit();

    const component = renderComponent();
    fireEvent.click(screen.getByTestId("rename"));
    expect(screen.getByTestId("rename")).toBeTruthy();

    expect(stateSpy).toHaveBeenCalledTimes(1);
    expect(stateSpy).toHaveBeenCalledWith(false);
    component.unmount();
  });

  it("should not open the modal if readOnly is true", () => {
    mockModal();

    const component = renderComponent(true);
    fireEvent.click(screen.getByTestId("rename"));
    expect(screen.queryByTestId("modal-archive-rename")).not.toBeTruthy();
    component.unmount();
  });
});