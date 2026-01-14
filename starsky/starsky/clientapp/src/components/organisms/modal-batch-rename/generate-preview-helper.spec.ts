import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { generatePreviewHelper } from "./generate-preview-helper";
import { IModalBatchRenameProps } from "./modal-batch-rename";

describe("generatePreviewHelper", () => {
  const mockSetIsPreviewLoading = jest.fn();
  const mockSetError = jest.fn();
  const mockSetPreview = jest.fn();
  const mockSetPreviewGenerated = jest.fn();
  const baseProps: IModalBatchRenameProps = {
    isOpen: true,
    handleExit: jest.fn(),
    select: ["/file1.jpg"],
    historyLocationSearch: "",
    state: { fileIndexItems: [] } as unknown as IArchiveProps,
    dispatch: jest.fn(),
    undoSelection: jest.fn()
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should set error if pattern is empty", async () => {
    await generatePreviewHelper(
      "   ",
      mockSetIsPreviewLoading,
      mockSetError,
      mockSetPreview,
      mockSetPreviewGenerated,
      baseProps
    );
    expect(mockSetError).toHaveBeenCalledWith("Pattern cannot be empty");
    expect(mockSetIsPreviewLoading).not.toHaveBeenCalled();
  });

  it("should set error if response status is not 200", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: [],
      statusCode: 500
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await generatePreviewHelper(
      "pattern",
      mockSetIsPreviewLoading,
      mockSetError,
      mockSetPreview,
      mockSetPreviewGenerated,
      baseProps
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(true);
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(false);
    expect(fetchPostSpy).toHaveBeenCalled();
  });

  it("should set preview and previewGenerated on success", async () => {
    const previewItems: IBatchRenameItem[] = [
      {
        sourceFilePath: "a",
        targetFilePath: "b",
        relatedFilePaths: [],
        sequenceNumber: 1,
        hasError: false
      }
    ];
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: previewItems,
      statusCode: 200
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await generatePreviewHelper(
      "pattern",
      mockSetIsPreviewLoading,
      mockSetError,
      mockSetPreview,
      mockSetPreviewGenerated,
      baseProps
    );
    expect(mockSetPreview).toHaveBeenCalledWith(previewItems);
    expect(mockSetPreviewGenerated).toHaveBeenCalledWith(true);
    expect(mockSetError).not.toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(true);
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(false);
    expect(fetchPostSpy).toHaveBeenCalled();
  });

  it("should set preview with empty relatedFilePaths", async () => {
    // set previewItems to null to simulate error in relatedFilePaths
    const previewItems: IBatchRenameItem[] = null as unknown as IBatchRenameItem[];
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      ...newIConnectionDefault(),
      data: previewItems,
      statusCode: 200
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await generatePreviewHelper(
      "pattern",
      mockSetIsPreviewLoading,
      mockSetError,
      mockSetPreview,
      mockSetPreviewGenerated,
      baseProps
    );
    expect(mockSetPreview).toHaveBeenCalledWith([]);
    expect(mockSetPreviewGenerated).toHaveBeenCalledWith(true);
    expect(mockSetError).not.toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(true);
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(false);
    expect(fetchPostSpy).toHaveBeenCalled();
  });

  it("should handle fetch error", async () => {
    const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
      throw new Error("fail");
    });
    await generatePreviewHelper(
      "pattern",
      mockSetIsPreviewLoading,
      mockSetError,
      mockSetPreview,
      mockSetPreviewGenerated,
      baseProps
    );
    expect(mockSetError).toHaveBeenCalledWith("Error generating preview");
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(true);
    expect(mockSetIsPreviewLoading).toHaveBeenCalledWith(false);
    expect(fetchPostSpy).toHaveBeenCalled();
  });
});
