import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../../shared/fetch/fetch-post";
import { IFileRenameState } from "../hooks/use-file-rename-state";
import { FileRenameMode } from "./render-file-rename-mode";

describe("FileRenameMode error cases", () => {
  const mockState = {
    fileIndexItems: [{ fileName: "test.jpg", filePath: "/test.jpg" }]
  } as unknown as IArchiveProps;
  const select = ["/test.jpg"];
  const collections = true;
  let dispatch: jest.Mock;
  let undoSelection: jest.Mock;
  let handleExit: jest.Mock;
  let fileRenameState: IFileRenameState;

  beforeEach(() => {
    dispatch = jest.fn();
    undoSelection = jest.fn();
    handleExit = jest.fn();
    fileRenameState = {
      shouldRename: true,
      setShouldRename: jest.fn(),
      renamePreview: [
        {
          sourceFilePath: "/test.jpg",
          targetFilePath: "/test_renamed.jpg",
          hasError: false,
          fileIndexItem: { fileName: "test.jpg", filePath: "/test.jpg" } as IFileIndexItem,
          detectedPatternDescription: "Test Rename",
          originalDateTime: new Date().toString(),
          correctedDateTime: new Date().toString(),
          relatedFilePaths: [],
          offsetHours: 1,
          errorMessage: "",
          warning: ""
        }
      ],
      setRenamePreview: jest.fn(),
      isLoadingRename: false,
      setIsLoadingRename: jest.fn(),
      isExecutingRename: false,
      setIsExecutingRename: jest.fn(),
      renameError: null,
      setRenameError: jest.fn()
    } as unknown as IFileRenameState;
  });

  it("shows error if mode/offsetData is invalid on execute", async () => {
    render(
      <FileRenameMode
        select={select}
        state={mockState}
        fileRenameState={fileRenameState}
        handleExit={handleExit}
        dispatch={dispatch}
        undoSelection={undoSelection}
        collections={collections}
        mode={"offset"}
        // offsetData is missing
      />
    );
    const button = screen.getByTestId("execute-rename-button");
    fireEvent.click(button);
    await waitFor(() => {
      expect(fileRenameState.setRenameError).toHaveBeenCalledWith("Invalid mode or missing data");
      expect(fileRenameState.setIsExecutingRename).toHaveBeenCalledWith(false);
    });
  });

  it("shows error if mode/timezoneData is invalid on execute", async () => {
    render(
      <FileRenameMode
        select={select}
        state={mockState}
        fileRenameState={fileRenameState}
        handleExit={handleExit}
        dispatch={dispatch}
        undoSelection={undoSelection}
        collections={collections}
        mode={"timezone"}
        // timezoneData is missing
      />
    );
    const button = screen.getByTestId("execute-rename-button");
    fireEvent.click(button);
    await waitFor(() => {
      expect(fileRenameState.setRenameError).toHaveBeenCalledWith("Invalid mode or missing data");
      expect(fileRenameState.setIsExecutingRename).toHaveBeenCalledWith(false);
    });
  });

  it("shows error if FetchPost returns non-200 or missing data", async () => {
    // Mock FetchPost to return non-200
    jest.spyOn(FetchPost, "default").mockResolvedValueOnce({ statusCode: 500, data: null });
    render(
      <FileRenameMode
        select={select}
        state={mockState}
        fileRenameState={fileRenameState}
        handleExit={handleExit}
        dispatch={dispatch}
        undoSelection={undoSelection}
        collections={collections}
        mode={"offset"}
        offsetData={{ year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 0 }}
      />
    );
    const button = screen.getByTestId("execute-rename-button");
    fireEvent.click(button);
    await waitFor(() => {
      expect(fileRenameState.setRenameError).toHaveBeenCalledWith("Failed to rename files");
      expect(fileRenameState.setIsExecutingRename).toHaveBeenCalledWith(false);
    });

    // Mock FetchPost to return 200 but no data
    jest.spyOn(FetchPost, "default").mockResolvedValueOnce({ statusCode: 200, data: null });
    render(
      <FileRenameMode
        select={select}
        state={mockState}
        fileRenameState={fileRenameState}
        handleExit={handleExit}
        dispatch={dispatch}
        undoSelection={undoSelection}
        collections={collections}
        mode={"offset"}
        offsetData={{ year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 0 }}
      />
    );
    fireEvent.click(screen.queryAllByTestId("execute-rename-button")[0]);
    await waitFor(() => {
      expect(fileRenameState.setRenameError).toHaveBeenCalledWith("Failed to rename files");
      expect(fileRenameState.setIsExecutingRename).toHaveBeenCalledWith(false);
    });
  });
});
