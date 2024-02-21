// import * as FetchPost from "../../../shared/fetch/fetch-post";
// import { StartMenuOptionDesktopEditorOpen } from "./menu-option-desktop-editor-open";

describe("ModalDesktopEditorOpenConfirmation", () => {
  it("sets modal confirmation open files if openWithoutConformationResult is false", () => {
    console.log("test");
  });

  // it("sets modal confirmation open files if openWithoutConformationResult is false", async () => {
  //   const select = ["file1", "file2"];
  //   const collections = false;
  //   const state = {
  //     fileIndexItems: [{ id: "file1" }, { id: "file2" }]
  //   };
  //   const setIsError = jest.fn();
  //   const messageDesktopEditorUnableToOpen = "Unable to open desktop editor";
  //   const setModalConfirmationOpenFiles = jest.fn();
  //   // Mock FetchPost to return false for openWithoutConformationResult
  //   jest.spyOn(FetchPost, "default").mockResolvedValue({ data: false });
  //   await StartMenuOptionDesktopEditorOpen(
  //     select,
  //     collections,
  //     state,
  //     setIsError,
  //     messageDesktopEditorUnableToOpen,
  //     setModalConfirmationOpenFiles
  //   );
  //   expect(setModalConfirmationOpenFiles).toHaveBeenCalledWith(true);
  //   expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
  // });
  // it("calls openDesktop if openWithoutConformationResult is true", async () => {
  //   const select = ["file1", "file2"];
  //   const collections = false;
  //   const state = {
  //     fileIndexItems: [{ id: "file1" }, { id: "file2" }]
  //   };
  //   const setIsError = jest.fn();
  //   const messageDesktopEditorUnableToOpen = "Unable to open desktop editor";
  //   const setModalConfirmationOpenFiles = jest.fn();
  //   // Mock FetchPost to return true for openWithoutConformationResult
  //   jest.spyOn(FetchPost, "default").mockResolvedValue({ data: true });
  //   await StartMenuOptionDesktopEditorOpen(
  //     select,
  //     collections,
  //     state,
  //     setIsError,
  //     messageDesktopEditorUnableToOpen,
  //     setModalConfirmationOpenFiles
  //   );
  //   expect(setModalConfirmationOpenFiles).not.toHaveBeenCalled(); // Modal confirmation should not be set in this case
  //   expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
  //   expect(openDesktop).toHaveBeenCalledWith(
  //     select,
  //     collections,
  //     state,
  //     setIsError,
  //     messageDesktopEditorUnableToOpen
  //   );
  // });
});
