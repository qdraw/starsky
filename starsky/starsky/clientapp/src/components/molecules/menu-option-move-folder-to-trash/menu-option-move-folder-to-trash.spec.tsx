import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import localization from "../../../localization/localization.json";
import * as Modal from "../../atoms/modal/modal";
import * as ModalMoveFolderToTrash from "../../organisms/modal-move-folder-to-trash/modal-move-folder-to-trash";
import MenuOptionMoveFolderToTrash from "./menu-option-move-folder-to-trash";

describe("MenuOptionMoveFolderToTrash", () => {
  it("renders the menu option correctly", () => {
    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
      />
    );

    expect(screen.getByText(localization.MessageMoveCurrentFolderToTrash.en)).toBeTruthy();
  });

  it("opens the modal when the menu option is clicked", () => {
    jest
      .spyOn(ModalMoveFolderToTrash, "default")
      .mockImplementationOnce(() => <div data-test="modal-move-folder-to-trash"></div>);

    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.click(menuOption);

    expect(screen.getByTestId("modal-move-folder-to-trash")).toBeTruthy();
  });

  it("opens the modal when the menu option is keyDowned", () => {
    jest
      .spyOn(ModalMoveFolderToTrash, "default")
      .mockImplementationOnce(() => <div data-test="modal-move-folder-to-trash"></div>);

    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.keyDown(menuOption, { key: "Enter" });

    expect(screen.getByTestId("modal-move-folder-to-trash")).toBeTruthy();
  });

  it("not opens the modal when the menu option is keyDowned but wrong key so ignored", () => {
    jest
      .spyOn(ModalMoveFolderToTrash, "default")
      .mockImplementationOnce(() => <div data-test="modal-move-folder-to-trash"></div>);

    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.keyDown(menuOption, { key: "Tab" });

    expect(screen.queryByTestId("modal-move-folder-to-trash")).toBeFalsy();
  });

  it("opens the modal when the menu option is clicked 1", () => {
    console.log("----------");

    const modalSpy = jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <>{props.children}</>;
    });

    jest.spyOn(ModalMoveFolderToTrash, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <></>;
    });

    const setEnableMoreMenuSpy = jest.fn();
    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
        setEnableMoreMenu={setEnableMoreMenuSpy}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.click(menuOption);

    expect(screen.getByTestId("move-folder-to-trash")).toBeTruthy();

    expect(setEnableMoreMenuSpy).toHaveBeenCalledTimes(1);
    expect(setEnableMoreMenuSpy).toHaveBeenCalledWith(false);

    expect(modalSpy).toHaveBeenCalledTimes(0);
  });

  it("opens the modal when the menu option is keyDown tab so skip", () => {
    console.log("----------");

    const modalSpy = jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <>{props.children}</>;
    });

    jest.spyOn(ModalMoveFolderToTrash, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <></>;
    });

    const setEnableMoreMenuSpy = jest.fn();
    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
        setEnableMoreMenu={setEnableMoreMenuSpy}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.keyDown(menuOption, { key: "Tab" });

    expect(screen.getByTestId("move-folder-to-trash")).toBeTruthy();

    expect(setEnableMoreMenuSpy).toHaveBeenCalledTimes(0);

    expect(modalSpy).toHaveBeenCalledTimes(0);
  });

  it("opens the modal when the menu option is keyDown enter 1", () => {
    console.log("----------");

    const modalSpy = jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <>{props.children}</>;
    });

    jest.spyOn(ModalMoveFolderToTrash, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit();
      });
      return <></>;
    });

    const setEnableMoreMenuSpy = jest.fn();
    render(
      <MenuOptionMoveFolderToTrash
        subPath="path/to/folder"
        isReadOnly={false}
        dispatch={jest.fn()}
        setEnableMoreMenu={setEnableMoreMenuSpy}
      />
    );

    const menuOption = screen.getByTestId("move-folder-to-trash");
    fireEvent.keyDown(menuOption, { key: "Enter" });

    expect(screen.getByTestId("move-folder-to-trash")).toBeTruthy();

    expect(setEnableMoreMenuSpy).toHaveBeenCalledTimes(1);
    expect(setEnableMoreMenuSpy).toHaveBeenCalledWith(false);

    expect(modalSpy).toHaveBeenCalledTimes(0);
  });
});
