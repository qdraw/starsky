import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { IExifStatus } from "../../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { IExifTimezoneCorrectionResult } from "../../../../interfaces/ITimezone";
import * as FetchPost from "../../../../shared/fetch/fetch-post";
import * as ClearSearchCache from "../../../../shared/search/clear-search-cache";
import * as URLPath from "../../../../shared/url/url-path";
import * as UrlQuery from "../../../../shared/url/url-query";
import { executeShift } from "./execute-shift";

describe("executeShift", () => {
  const mockFileIndexItem = {
    fileName: "test.jpg",
    filePath: "/test.jpg",
    dateTime: "2024-08-12T14:32:00",
    colorClass: 0,
    fileCollectionName: "test",
    fileHash: "abc123",
    parentDirectory: "/",
    status: IExifStatus.Default
  };

  const mockState = {
    fileIndexItems: [mockFileIndexItem],
    relativeObjects: {},
    subPath: "",
    breadcrumb: [],
    colorClassActiveList: [],
    colorClassUsage: [],
    collectionsCount: 0,
    pageType: "Archive",
    isReadOnly: false,
    dateCache: 0
  } as unknown as IArchiveProps;

  let mockSetIsExecuting: jest.Mock;
  let mockSetError: jest.Mock;
  let mockHandleExit: jest.Mock;
  let mockDispatch: jest.Mock;

  beforeEach(() => {
    mockSetIsExecuting = jest.fn();
    mockSetError = jest.fn();
    mockHandleExit = jest.fn();
    mockDispatch = jest.fn();
    jest.spyOn(console, "log").mockImplementation(() => {});
    jest.spyOn(console, "error").mockImplementation(() => {});
  });

  afterEach(() => {
    jest.clearAllMocks();
    jest.restoreAllMocks();
  });

  describe("early returns and validation", () => {
    it("returns early when select array is empty", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: []
      });

      await executeShift(
        {
          select: [],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetIsExecuting).not.toHaveBeenCalled();
      expect(FetchPost.default).not.toHaveBeenCalled();
    });
  });

  describe("offset mode", () => {
    beforeEach(() => {
      jest.spyOn(URLPath, "URLPath").mockImplementation(
        () =>
          ({
            MergeSelectFileIndexItem: jest.fn().mockReturnValue(["/test.jpg"]),
            encodeURI: jest.fn().mockReturnValue("/test.jpg")
          }) as unknown as URLPath.URLPath
      );

      jest.spyOn(UrlQuery, "UrlQuery").mockImplementation(
        () =>
          ({
            UrlOffsetExecute: jest.fn().mockReturnValue("/api/offset-execute"),
            UrlTimezoneExecute: jest.fn().mockReturnValue("/api/timezone-execute")
          }) as unknown as UrlQuery.UrlQuery
      );
    });

    it("successfully executes offset shift with all offset data", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };
      const clearSearchCacheSpy = jest
        .spyOn(ClearSearchCache, "ClearSearchCache")
        .mockImplementationOnce(() => {});
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          offsetData: {
            year: 1,
            month: 2,
            day: 3,
            hour: 4,
            minute: 5,
            second: 6
          },
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(1, true);
      expect(mockSetError).toHaveBeenCalledWith(null);
      expect(FetchPost.default).toHaveBeenCalledWith(
        "/api/offset-execute?f=/test.jpg&collections=true",
        JSON.stringify({
          year: 1,
          month: 2,
          day: 3,
          hour: 4,
          minute: 5,
          second: 6
        }),
        "post",
        { "Content-Type": "application/json" }
      );
      expect(mockDispatch).toHaveBeenCalledWith({
        type: "add",
        add: [mockFileIndexItem]
      });
      expect(mockHandleExit).toHaveBeenCalled();
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(2, false);
      expect(clearSearchCacheSpy).toHaveBeenCalled();
    });

    it("successfully executes offset shift with partial offset data", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });
      const clearSearchCacheSpy = jest
        .spyOn(ClearSearchCache, "ClearSearchCache")
        .mockImplementationOnce(() => {});
      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          offsetData: {
            year: 0,
            month: 0,
            day: 0,
            hour: 2,
            minute: 0,
            second: 0
          },
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(FetchPost.default).toHaveBeenCalledWith(
        "/api/offset-execute?f=/test.jpg&collections=true",
        JSON.stringify({
          year: 0,
          month: 0,
          day: 0,
          hour: 2,
          minute: 0,
          second: 0
        }),
        "post",
        { "Content-Type": "application/json" }
      );
      expect(mockDispatch).toHaveBeenCalled();
      expect(mockHandleExit).toHaveBeenCalled();
      expect(clearSearchCacheSpy).toHaveBeenCalled();
    });

    it("successfully executes offset shift with no offset data (defaults to 0)", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T14:32:00",
        delta: "+00:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };
      const clearSearchCacheSpy = jest
        .spyOn(ClearSearchCache, "ClearSearchCache")
        .mockImplementationOnce(() => {});
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(FetchPost.default).toHaveBeenCalledWith(
        "/api/offset-execute?f=/test.jpg&collections=true",
        JSON.stringify({
          year: 0,
          month: 0,
          day: 0,
          hour: 0,
          minute: 0,
          second: 0
        }),
        "post",
        { "Content-Type": "application/json" }
      );
      expect(mockDispatch).toHaveBeenCalled();
      expect(mockHandleExit).toHaveBeenCalled();
      expect(clearSearchCacheSpy).toHaveBeenCalled();
    });

    it("successfully executes offset shift with multiple files", async () => {
      const mockResult1: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      const mockFile2 = { ...mockFileIndexItem, fileName: "test2.jpg" };
      const mockResult2: IExifTimezoneCorrectionResult = {
        ...mockResult1,
        fileIndexItem: mockFile2
      };

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult1, mockResult2]
      });
      const clearSearchCacheSpy = jest
        .spyOn(ClearSearchCache, "ClearSearchCache")
        .mockImplementationOnce(() => {});
      await executeShift(
        {
          select: ["/test.jpg", "/test2.jpg"],
          state: mockState,
          isOffset: true,
          offsetData: {
            year: 1,
            month: 0,
            day: 0,
            hour: 0,
            minute: 0,
            second: 0
          },
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockDispatch).toHaveBeenCalledWith({
        type: "add",
        add: [mockFileIndexItem, mockFile2]
      });
      expect(mockHandleExit).toHaveBeenCalled();
      expect(clearSearchCacheSpy).toHaveBeenCalled();
    });
  });

  describe("timezone mode", () => {
    beforeEach(() => {
      jest.spyOn(URLPath, "URLPath").mockImplementation(
        () =>
          ({
            MergeSelectFileIndexItem: jest.fn().mockReturnValue(["/test.jpg"]),
            encodeURI: jest.fn().mockReturnValue("/test.jpg")
          }) as unknown as URLPath.URLPath
      );

      jest.spyOn(UrlQuery, "UrlQuery").mockImplementation(
        () =>
          ({
            UrlOffsetExecute: jest.fn().mockReturnValue("/api/offset-execute"),
            UrlTimezoneExecute: jest.fn().mockReturnValue("/api/timezone-execute")
          }) as unknown as UrlQuery.UrlQuery
      );
    });

    it("successfully executes timezone shift with all timezone data", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T20:32:00",
        delta: "+06:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };
      const clearSearchCacheSpy = jest
        .spyOn(ClearSearchCache, "ClearSearchCache")
        .mockImplementationOnce(() => {});
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: false,
          timezoneData: {
            recordedTimezoneId: "America/New_York",
            correctTimezoneId: "Europe/Amsterdam"
          },
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(FetchPost.default).toHaveBeenCalledWith(
        "/api/timezone-execute?f=/test.jpg&collections=true",
        JSON.stringify({
          recordedTimezoneId: "America/New_York",
          correctTimezoneId: "Europe/Amsterdam"
        }),
        "post",
        { "Content-Type": "application/json" }
      );
      expect(mockDispatch).toHaveBeenCalled();
      expect(mockHandleExit).toHaveBeenCalled();
      expect(clearSearchCacheSpy).toHaveBeenCalled();
    });

    it("successfully executes timezone shift with no timezone data (defaults to empty strings)", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T14:32:00",
        delta: "+00:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      jest.spyOn(ClearSearchCache, "ClearSearchCache").mockImplementationOnce(() => {});

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: false,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(FetchPost.default).toHaveBeenCalledWith(
        "/api/timezone-execute?f=/test.jpg&collections=true",
        JSON.stringify({
          recordedTimezoneId: "",
          correctTimezoneId: ""
        }),
        "post",
        { "Content-Type": "application/json" }
      );
      expect(mockDispatch).toHaveBeenCalled();
      expect(mockHandleExit).toHaveBeenCalled();
    });
  });

  describe("error handling", () => {
    beforeEach(() => {
      jest.spyOn(URLPath, "URLPath").mockImplementation(
        () =>
          ({
            MergeSelectFileIndexItem: jest.fn().mockReturnValue(["/test.jpg"]),
            encodeURI: jest.fn().mockReturnValue("/test.jpg")
          }) as unknown as URLPath.URLPath
      );

      jest.spyOn(UrlQuery, "UrlQuery").mockImplementation(
        () =>
          ({
            UrlOffsetExecute: jest.fn().mockReturnValue("/api/offset-execute"),
            UrlTimezoneExecute: jest.fn().mockReturnValue("/api/timezone-execute")
          }) as unknown as UrlQuery.UrlQuery
      );
    });

    it("sets error when response status code is not 200", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 500,
        data: []
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
      expect(mockHandleExit).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(2, false);
    });

    it("sets error when response data is not an array", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: { success: true }
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
      expect(mockHandleExit).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(2, false);
    });

    it("sets error when response data is null", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: null
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
      expect(mockHandleExit).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
    });

    it("sets error when response data is undefined", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: undefined
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
      expect(mockHandleExit).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
    });

    it("sets error when FetchPost throws an error", async () => {
      jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("Network error"));

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
      expect(mockHandleExit).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(2, false);
    });

    it("ensures setIsExecuting is set to false in finally block even on error", async () => {
      jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("Network error"));

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      const calls = mockSetIsExecuting.mock.calls;
      expect(calls[0]).toEqual([true]);
      expect(calls[calls.length - 1]).toEqual([false]);
    });

    it("sets error when response status is 400", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 400,
        data: []
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
    });

    it("sets error when response status is 401", async () => {
      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 401,
        data: []
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledWith("Failed to execute shift");
    });
  });

  describe("callback invocations", () => {
    beforeEach(() => {
      jest.spyOn(URLPath, "URLPath").mockImplementation(
        () =>
          ({
            MergeSelectFileIndexItem: jest.fn().mockReturnValue(["/test.jpg"]),
            encodeURI: jest.fn().mockReturnValue("/test.jpg")
          }) as unknown as URLPath.URLPath
      );

      jest.spyOn(UrlQuery, "UrlQuery").mockImplementation(
        () =>
          ({
            UrlOffsetExecute: jest.fn().mockReturnValue("/api/offset-execute"),
            UrlTimezoneExecute: jest.fn().mockReturnValue("/api/timezone-execute")
          }) as unknown as UrlQuery.UrlQuery
      );
    });

    it("calls setIsExecuting(true) at start and setIsExecuting(false) at end", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetIsExecuting).toHaveBeenCalledTimes(2);
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(1, true);
      expect(mockSetIsExecuting).toHaveBeenNthCalledWith(2, false);
    });

    it("calls setError(null) at start and only on error afterwards", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockSetError).toHaveBeenCalledTimes(2);
      expect(mockSetError).toHaveBeenCalledWith(null);
    });

    it("calls handleExit only on success", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };
      jest.spyOn(ClearSearchCache, "ClearSearchCache").mockImplementationOnce(() => {});

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockHandleExit).toHaveBeenCalledTimes(1);
    });

    it("does not call handleExit on error", async () => {
      jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("Network error"));

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockHandleExit).not.toHaveBeenCalled();
    });

    it("calls dispatch with correct action on success", async () => {
      const mockResult: IExifTimezoneCorrectionResult = {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: "",
        fileIndexItem: mockFileIndexItem
      };

      jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: [mockResult]
      });

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockDispatch).toHaveBeenCalledWith({
        type: "add",
        add: [mockFileIndexItem]
      });
    });

    it("does not call dispatch on error", async () => {
      jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("Network error"));

      await executeShift(
        {
          select: ["/test.jpg"],
          state: mockState,
          isOffset: true,
          historyLocationSearch: ""
        },
        mockSetIsExecuting,
        mockSetError,
        mockHandleExit,
        jest.fn(),
        mockDispatch,
        true
      );

      expect(mockDispatch).not.toHaveBeenCalled();
    });
  });

  describe.each([[true], [false]])(
    "calls handleExit only on success when collections=%s",
    (collections) => {
      it("should call handleExit and use correct collections param", async () => {
        const select = ["file1.jpg", "file2.jpg"];
        const state: IArchiveProps = {
          fileIndexItems: [
            { fileName: "file1.jpg", filePath: "/test/file1.jpg", parentDirectory: "/test" },
            { fileName: "file2.jpg", filePath: "/test/file2.jpg", parentDirectory: "/test" }
          ] as unknown as IFileIndexItem[],
          collections: collections
        } as unknown as IArchiveProps;
        const isOffset = false;
        const historyLocationSearch = "?q=test";
        const timezoneData = {
          recordedTimezoneId: "Europe/Amsterdam",
          correctTimezoneId: "Europe/Berlin"
        };

        // Mocks
        const setIsExecuting = jest.fn();
        const setError = jest.fn();
        const handleExit = jest.fn();
        const undoSelection = jest.fn();
        const dispatch = jest.fn();

        // Mock FetchPost
        const filePostSpy = jest.spyOn(FetchPost, "default").mockResolvedValue({
          statusCode: 200,
          data: [
            {
              fileIndexItem: {
                fileName: "file1.jpg",
                filePath: "/test/file1.jpg",
                parentDirectory: "/test"
              },
              success: true,
              error: "",
              warning: "",
              originalDateTime: "2020-01-01T00:00:00Z",
              correctedDateTime: "2020-01-01T01:00:00Z",
              delta: "3600"
            },
            {
              fileIndexItem: {
                fileName: "file2.jpg",
                filePath: "/test/file2.jpg",
                parentDirectory: "/test"
              },
              success: true,
              error: "",
              warning: "",
              originalDateTime: "2020-01-01T00:00:00Z",
              correctedDateTime: "2020-01-01T01:00:00Z",
              delta: "3600"
            }
          ] as IExifTimezoneCorrectionResult[]
        });

        // Mock ClearSearchCache
        jest.spyOn(ClearSearchCache, "ClearSearchCache").mockImplementation(jest.fn());

        await executeShift(
          { select, state, isOffset, timezoneData, historyLocationSearch },
          setIsExecuting,
          setError,
          handleExit,
          undoSelection,
          dispatch,
          collections
        );

        console.log(collections);

        expect(filePostSpy).toHaveBeenCalledWith(
          `/starsky/api/meta-time-correct/timezone-execute?f=/test/file1.jpg;/test/file2.jpg&collections=${collections}`,
          JSON.stringify({
            recordedTimezoneId: "Europe/Amsterdam",
            correctTimezoneId: "Europe/Berlin"
          }),
          "post",
          { "Content-Type": "application/json" }
        );

        expect(setIsExecuting).toHaveBeenCalledWith(true);
        expect(setError).toHaveBeenCalledWith(null);
        expect(dispatch).toHaveBeenCalledWith({
          type: "add",
          add: [
            { fileName: "file1.jpg", filePath: "/test/file1.jpg", parentDirectory: "/test" },
            { fileName: "file2.jpg", filePath: "/test/file2.jpg", parentDirectory: "/test" }
          ]
        });
        expect(handleExit).toHaveBeenCalled();
        expect(setIsExecuting).toHaveBeenCalledWith(false);
      });
    }
  );
});
