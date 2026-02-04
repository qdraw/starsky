import { fireEvent, render, screen } from "@testing-library/react";
import React from "react";
import * as ModalMoveFile from "../../organisms/modal-move-file/modal-move-file";
import MenuOptionMoveFile from "./menu-option-move-file";

describe("MenuOptionMoveFile", () => {
  let modalSpy = jest.spyOn(ModalMoveFile, "default");

  function mockModalHandleExit() {
    modalSpy.mockReset();
    modalSpy = jest.spyOn(ModalMoveFile, "default").mockImplementation((props) => {
      props.handleExit();
      return <h1 data-test="modal-archive-mkdir">test</h1>;
    });
  }

  const renderComponent = (readOnly = false) => {
    return render(
      <MenuOptionMoveFile isReadOnly={readOnly} subPath={"/test"} parentDirectory="/test" />
    );
  };

  function mockModal() {
    modalSpy.mockReset();
    modalSpy = jest.spyOn(ModalMoveFile, "default").mockImplementation(() => {
      return <h1 data-test="modal-move-file">test</h1>;
    });
  }

  it("should not show the modal initially", () => {
    mockModal();
    const component = renderComponent();
    expect(screen.queryByTestId("modal-move-file")).not.toBeTruthy();
    expect(modalSpy).toHaveBeenCalledTimes(0);
    component.unmount();
  });

  it("should open the modal when the button is clicked", () => {
    mockModal();
    const component = renderComponent();
    fireEvent.click(screen.getByTestId("move"));
    expect(modalSpy).toHaveBeenCalledTimes(1);

    expect(screen.getByTestId("modal-move-file")).toBeTruthy();
    component.unmount();
  });

  it("should close the modal when the handleExit function is called", () => {
    const stateSpy = jest.spyOn(React, "useState").mockReturnValueOnce([true, jest.fn()]);
    mockModalHandleExit();

    const component = renderComponent();
    fireEvent.click(screen.getByTestId("move"));
    expect(screen.getByTestId("move")).toBeTruthy();

    // Simulate closing the modal
    fireEvent.click(screen.getByTestId("move"));

    expect(stateSpy).toHaveBeenCalledTimes(1);
    expect(stateSpy).toHaveBeenCalledWith(false);
    component.unmount();
  });
});
