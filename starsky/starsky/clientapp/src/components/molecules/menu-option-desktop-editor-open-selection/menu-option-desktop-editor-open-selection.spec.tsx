import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType, newIRelativeObjects } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { StartMenuOptionDesktopEditorOpenSelection } from "./menu-option-desktop-editor-open-selection";

describe("ModalDesktopEditorOpenConfirmation", () => {
  const state = {
    fileIndexItems: [
      {
        filePath: "/file2.jpg",
        fileName: "file1",
        fileCollectionName: "1",
        fileHash: "1",
        parentDirectory: "/",
        status: IExifStatus.Ok
      },
      {
        filePath: "/file2.jpg",
        fileName: "file2",
        fileCollectionName: "1",
        fileHash: "1",
        parentDirectory: "/",
        status: IExifStatus.Ok
      }
    ],
    relativeObjects: newIRelativeObjects(),
    subPath: "",
    breadcrumb: [],
    colorClassUsage: [],
    collectionsCount: 1,
    colorClassActiveList: [],
    pageType: PageType.Archive,
    isReadOnly: false,
    dateCache: 1
  } as IArchiveProps;

  it("sets modal confirmation open files if openWithoutConformationResult is false", async () => {
    const select = ["file1", "file2"];
    const collections = false;

    const setIsError = jest.fn();
    const messageDesktopEditorUnableToOpen = "Unable to open desktop editor";
    const setModalConfirmationOpenFiles = jest.fn();

    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: false,
      statusCode: 200
    } as IConnectionDefault);

    jest.spyOn(FetchPost, "default").mockImplementationOnce(() => mockIConnectionDefaultResolve);

    await StartMenuOptionDesktopEditorOpenSelection(
      select,
      collections,
      state,
      setIsError,
      messageDesktopEditorUnableToOpen,
      setModalConfirmationOpenFiles
    );
    expect(setModalConfirmationOpenFiles).toHaveBeenCalledWith(true);
    expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
  });
  xit("calls openDesktop if openWithoutConformationResult is true", async () => {
    const select = ["file1", "file2"];
    const collections = false;
    const setIsError = jest.fn();
    const messageDesktopEditorUnableToOpen = "Unable to open desktop editor";
    const setModalConfirmationOpenFiles = jest.fn();
    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: false,
      statusCode: 200
    } as IConnectionDefault);

    jest.spyOn(FetchPost, "default").mockImplementationOnce(() => mockIConnectionDefaultResolve);

    await StartMenuOptionDesktopEditorOpenSelection(
      select,
      collections,
      state,
      setIsError,
      messageDesktopEditorUnableToOpen,
      setModalConfirmationOpenFiles
    );
    expect(setModalConfirmationOpenFiles).not.toHaveBeenCalled(); // Modal confirmation should not be set in this case
    expect(setIsError).not.toHaveBeenCalled(); // Error should not be set in this case
    // expect(openDesktop).toHaveBeenCalledWith(
    //   select,
    //   collections,
    //   state,
    //   setIsError,
    //   messageDesktopEditorUnableToOpen
    // );
  });
});
