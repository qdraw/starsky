import { fireEvent, render, screen } from "@testing-library/react";
import localization from "../../../localization/localization.json";
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

    expect(
      screen.getByText(localization.MessageMoveCurrentFolderToTrash.en)
    ).toBeTruthy();
  });

  it("opens the modal when the menu option is clicked", () => {
    jest
      .spyOn(ModalMoveFolderToTrash, "default")
      .mockImplementationOnce(() => (
        <div data-test="modal-move-folder-to-trash"></div>
      ));

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
});
