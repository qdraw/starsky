import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import * as FileListCache from "../../../shared/filelist-cache";
import { executeBatchRenameHelper } from "./execute-batch-rename-helper";
import { IModalBatchRenameProps } from "./modal-batch-rename";

// Mocks
const mockSetError = jest.fn();
const mockSetIsLoading = jest.fn();
const mockSetRecentPatterns = jest.fn();
const mockHandleExit = jest.fn();
const mockUndoSelection = jest.fn();
const mockDispatch = jest.fn();

const baseProps: IModalBatchRenameProps = {
  isOpen: true,
  handleExit: mockHandleExit,
  select: ["/file1.jpg"],
  historyLocationSearch: "",
  state: { fileIndexItems: [] } as any,
  dispatch: mockDispatch,
  undoSelection: mockUndoSelection
};

describe("executeBatchRenameHelper", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it("should set error if preview not generated", async () => {
    await executeBatchRenameHelper(
      mockSetError,
      [],
      mockSetIsLoading,
      baseProps,
      "pattern",
      [],
      mockSetRecentPatterns
    );
    expect(mockSetError).toHaveBeenCalledWith("Please generate a preview first");
  });

  it("should set error if preview has errors", async () => {
    const preview: IBatchRenameItem[] = [{ hasError: true } as unknown as IBatchRenameItem];
    await executeBatchRenameHelper(
      mockSetError,
      preview,
      mockSetIsLoading,
      baseProps,
      "pattern",
      [],
      mockSetRecentPatterns
    );
    expect(mockSetError).toHaveBeenCalledWith("Cannot rename: there are errors in the preview");
  });

  it("should set error if response status is not 200", async () => {
    const preview: IBatchRenameItem[] = [{ hasError: false } as unknown as IBatchRenameItem];
    await executeBatchRenameHelper(
      mockSetError,
      preview,
      mockSetIsLoading,
      baseProps,
      "pattern",
      [],
      mockSetRecentPatterns
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to execute batch rename");
  });

  it("should update recent patterns and call side effects on success", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: [
        {
          sourceFilePath: "/test1.jpg",
          targetFilePath: "/renamed_test1.jpg",
          relatedFilePaths: [],
          sequenceNumber: 1,
          hasError: false,
          errorMessage: undefined
        }
      ],
      statusCode: 200
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const preview: IBatchRenameItem[] = [{ hasError: false } as unknown as IBatchRenameItem];
    const pattern = "pattern1";
    const recentPatterns = ["pattern2", "pattern1", "pattern3"];
    await executeBatchRenameHelper(
      mockSetError,
      preview,
      mockSetIsLoading,
      baseProps,
      pattern,
      recentPatterns,
      mockSetRecentPatterns
    );
    expect(mockSetRecentPatterns).toHaveBeenCalled();
    expect(fetchPostSpy).toHaveBeenCalled();
    expect(mockHandleExit).toHaveBeenCalled();
    expect(mockUndoSelection).toHaveBeenCalled();
    expect(mockDispatch).toHaveBeenCalled();
  });

  it("should handle fetch error", async () => {
    const preview: IBatchRenameItem[] = [{ hasError: false } as unknown as IBatchRenameItem];
    await executeBatchRenameHelper(
      mockSetError,
      preview,
      mockSetIsLoading,
      baseProps,
      "pattern",
      [],
      mockSetRecentPatterns
    );
    expect(mockSetError).toHaveBeenCalledTimes(2);
    expect(mockSetError).toHaveBeenCalledWith("Failed to execute batch rename");
  });

  it("should handle fetch error 2", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: { test: "data" },
      statusCode: 200
    });

    jest.spyOn(FileListCache, "FileListCache").mockImplementationOnce(() => {
      return new Error("Fetch error") as unknown as FileListCache.FileListCache;
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const preview: IBatchRenameItem[] = [{ hasError: false } as unknown as IBatchRenameItem];
    await executeBatchRenameHelper(
      mockSetError,
      preview,
      mockSetIsLoading,
      baseProps,
      "pattern",
      [],
      mockSetRecentPatterns
    );
    expect(mockSetError).toHaveBeenCalledTimes(2);
    expect(mockSetError).toHaveBeenNthCalledWith(2, "Error executing batch rename");
    expect(fetchPostSpy).toHaveBeenCalled();
  });
});
