import { IExifTimezoneCorrectionResultContainer } from "../../../../interfaces/ITimezone";
import * as FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";
import { generateOffsetPreview, IOffset } from "./generate-offset-preview";

describe("generateOffsetPreview", () => {
  const mockSetIsLoadingPreview = jest.fn();
  const mockSetError = jest.fn();
  const mockSetPreview = jest.fn();

  const mockOffset: IOffset = {
    year: 1,
    month: 2,
    day: 3,
    hour: 4,
    minute: 5,
    second: 6
  };

  const mockPreview: IExifTimezoneCorrectionResultContainer = {
    offsetData: [],
    timezoneData: []
  };

  const mockState = {
    fileIndexItems: ["/path/file1.jpg", "/path/file2.jpg"],
    collections: true
  } as unknown as any;

  beforeEach(() => {
    jest.clearAllMocks();
    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: [{ filePath: "/path/file1.jpg", dateTime: "2023-01-01" }]
    });
    jest.spyOn(URLPath.prototype, "MergeSelectFileIndexItem").mockReturnValue(["/path/file1.jpg"]);
    jest.spyOn(URLPath.prototype, "encodeURI").mockImplementation((val) => val);
    jest.spyOn(UrlQuery.prototype, "UrlOffsetPreview").mockReturnValue("/api/offset-preview");
  });

  it("should return early if select is empty", async () => {
    await generateOffsetPreview(
      [],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetIsLoadingPreview).not.toHaveBeenCalled();
  });

  it("should set loading state and clear error on start", async () => {
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetIsLoadingPreview).toHaveBeenCalledWith(true);
    expect(mockSetError).toHaveBeenCalledWith(null);
  });

  it("should fetch preview with correct parameters", async () => {
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(FetchPost.default).toHaveBeenCalled();
    const mockFetchPost = jest.spyOn(FetchPost, "default");
    const call = mockFetchPost.mock.calls[0];
    expect(call[0]).toContain("/api/offset-preview");
    expect(call[0]).toContain("collections=true");
    expect(call[1]).toContain('"year":1');
    expect(call[1]).toContain('"month":2');
  });

  it("should set preview data on successful response", async () => {
    const offsetData = [{ filePath: "/path/file1.jpg", dateTime: "2023-01-01" }];
    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: offsetData
    });
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetPreview).toHaveBeenCalledWith({
      ...mockPreview,
      offsetData
    });
    expect(mockSetError).toHaveBeenCalledWith(null);
  });

  it("should handle non-200 status code", async () => {
    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 500,
      data: []
    });
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetPreview).toHaveBeenCalledWith({
      ...mockPreview,
      offsetData: []
    });
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
  });

  it("should handle non-array response data", async () => {
    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: { invalidData: "not an array" }
    });
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetPreview).toHaveBeenCalledWith({
      ...mockPreview,
      offsetData: []
    });
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
  });

  it("should handle network error", async () => {
    const error = new Error("Network error");
    jest.spyOn(FetchPost, "default").mockRejectedValue(error);
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
  });

  it("should always set loading to false in finally block", async () => {
    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: []
    });
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetIsLoadingPreview).toHaveBeenLastCalledWith(false);
  });

  it("should set loading to false even on error", async () => {
    jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("Error"));
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(mockSetIsLoadingPreview).toHaveBeenLastCalledWith(false);
  });

  it("should handle state without collections", async () => {
    const stateWithoutCollections = { ...mockState, collections: false };
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      stateWithoutCollections,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    const mockFetchPost = jest.spyOn(FetchPost, "default");
    const call = mockFetchPost.mock.calls[0];
    expect(call[0]).toContain("collections=false");
  });

  it("should use first file from merged list", async () => {
    await generateOffsetPreview(
      ["/path/file1.jpg"],
      mockState,
      mockOffset,
      mockSetIsLoadingPreview,
      mockSetError,
      mockPreview,
      mockSetPreview
    );
    expect(encodeURISpy).toHaveBeenCalledWith("/path/file1.jpg");
  });
});
