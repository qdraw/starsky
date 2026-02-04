import { IDetailView } from "../../../../interfaces/IDetailView";
import { IFileIndexItem, Orientation } from "../../../../interfaces/IFileIndexItem";
import * as FetchGet from "../../../../shared/fetch/fetch-get.ts"; // for expect assertions
import { RequestNewFileHash } from "./request-new-filehash.ts";

describe("requestNewFileHash", () => {
  const mockState: IDetailView = {
    subPath: "/path/to/image.jpg",
    fileIndexItem: {
      fileHash: "abc123",
      filePath: "/path/to/image.jpg",
      orientation: Orientation.Horizontal
    } as unknown as IFileIndexItem
  } as unknown as IDetailView;

  const mockSetIsLoading = jest.fn();
  const mockDispatch = jest.fn();

  let fetchGetSpy: jest.SpyInstance;

  beforeEach(() => {
    fetchGetSpy = jest.spyOn(FetchGet, "default");
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("returns null if FetchGet fails", async () => {
    fetchGetSpy.mockResolvedValueOnce(null);

    const result = await RequestNewFileHash(mockState, mockSetIsLoading, mockDispatch);

    expect(result).toBeNull();
    expect(mockSetIsLoading).toHaveBeenCalledTimes(0);
  });

  it("returns null if FetchGet status code is not 200", async () => {
    const mockResult = {
      statusCode: 404 // or any non-200 status code
    };
    fetchGetSpy.mockResolvedValueOnce(mockResult);

    const result = await RequestNewFileHash(mockState, mockSetIsLoading, mockDispatch);

    expect(result).toBeNull();
    expect(mockSetIsLoading).toHaveBeenCalledWith(false);
  });

  it("updates context and returns true when fileHash changes", async () => {
    const mockResult = {
      statusCode: 200,
      data: {
        fileIndexItem: {
          fileHash: "test"
        },
        pageType: "DetailView"
      }
    };
    fetchGetSpy.mockResolvedValueOnce(mockResult);

    const result = await RequestNewFileHash(mockState, mockSetIsLoading, mockDispatch);

    expect(result).toBe(true);
    expect(mockDispatch).toHaveBeenCalledWith({
      fileHash: "test",
      filePath: undefined,
      orientation: "Horizontal",
      type: "update"
    });
    expect(mockSetIsLoading).toHaveBeenCalledWith(false);
  });

  it("returns false and updates context when fileHash remains the same", async () => {
    const mockResult = {
      statusCode: 200,
      data: {
        fileIndexItem: {
          fileHash: "abc123" // same as mockState.fileIndexItem.fileHash
        },
        pageType: "DetailView"
      }
    };
    fetchGetSpy.mockResolvedValueOnce(mockResult);

    const result = await RequestNewFileHash(mockState, mockSetIsLoading, mockDispatch);

    expect(result).toBe(false);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    expect(mockSetIsLoading).toHaveBeenCalledTimes(0);
  });
});
