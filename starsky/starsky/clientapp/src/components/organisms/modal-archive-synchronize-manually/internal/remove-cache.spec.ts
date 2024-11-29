import { waitFor } from "@testing-library/react";
import { PageType } from "../../../../interfaces/IDetailView";
import * as FetchGet from "../../../../shared/fetch/fetch-get";
import { FileListCache } from "../../../../shared/filelist-cache";
import { RemoveCache } from "./remove-cache";
import { CacheControl } from "../../../../shared/fetch/cache-control.ts";

describe("RemoveCache function", () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.clearAllTimers();
  });

  it("should call setIsLoading with true", () => {
    const setIsLoading = jest.fn();
    RemoveCache(setIsLoading, "/parent", "search", jest.fn(), jest.fn());
    expect(setIsLoading).toHaveBeenCalledWith(true);
  });

  it("should call CacheCleanEverything", () => {
    const cacheCleanEverything = jest.spyOn(FileListCache.prototype, "CacheCleanEverything");
    RemoveCache(jest.fn(), "/parent", "search", jest.fn(), jest.fn());
    expect(cacheCleanEverything).toHaveBeenCalled();
    cacheCleanEverything.mockRestore();
  });

  it("should call FetchGet with the correct URL to remove cache", () => {
    const mockFetchGet = jest
      .spyOn(FetchGet, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve({ statusCode: 200, data: null }));
    const parentFolder = "/parent";
    RemoveCache(jest.fn(), parentFolder, "search", jest.fn(), jest.fn());
    expect(mockFetchGet).toHaveBeenCalledWith("/starsky/api/remove-cache?json=true&f=/parent", {
      CacheControl
    });
    expect(mockFetchGet.mock.calls[0][0]).toContain("/remove-cache");
    expect(mockFetchGet.mock.calls[0][0]).toContain(
      "/starsky/api/remove-cache?json=true&f=/parent"
    );
    mockFetchGet.mockRestore();
  });

  it("should call FetchGet with invalid url to remove cache", () => {
    const mockFetchGet = jest
      .spyOn(FetchGet, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve({ statusCode: 200, data: null }));

    const parent: string | undefined = undefined as unknown as string;

    RemoveCache(jest.fn(), parent, "search", jest.fn(), jest.fn());
    expect(mockFetchGet).toHaveBeenCalledWith("/starsky/api/remove-cache?json=true&f=/", {
      CacheControl
    });
    expect(mockFetchGet.mock.calls[0][0]).toContain("/remove-cache");
    expect(mockFetchGet.mock.calls[0][0]).toContain("/starsky/api/remove-cache?json=true&f=/");
    mockFetchGet.mockRestore();
  });

  it("should call FetchGet with the correct URL to index server API after setTimeout", async () => {
    const mockFetchGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => Promise.resolve({ statusCode: 200, data: null }));

    const parentFolder = "/parent";
    const historyLocationSearch = "search";
    RemoveCache(jest.fn(), parentFolder, historyLocationSearch, jest.fn(), jest.fn());

    jest.runAllTimers();

    expect(mockFetchGet).toHaveBeenCalledWith("/starsky/api/remove-cache?json=true&f=/parent", {
      CacheControl
    });

    mockFetchGet.mockRestore();
  });

  it("should dispatch force-reset action when payload has fileIndexItems", async () => {
    const dispatch = jest.fn();
    const propsHandleExit = jest.fn();

    const mediaArchiveData = {
      data: {
        pageType: PageType.Archive,
        fileIndexItems: [{}, {}] // Mocking fileIndexItems
      },
      statusCode: 200
    };
    const mockFetchGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => Promise.resolve(mediaArchiveData))
      .mockImplementationOnce(() => Promise.resolve(mediaArchiveData));

    jest.useFakeTimers();

    RemoveCache(jest.fn(), "/parent", "search", dispatch, propsHandleExit);

    jest.advanceTimersByTime(600);

    await waitFor(() => {
      expect(propsHandleExit).toHaveBeenCalled();

      expect(dispatch).toHaveBeenCalledWith({
        type: "force-reset",
        payload: mediaArchiveData.data
      });
    });

    mockFetchGet.mockRestore();
  });

  it("should not dispatch force-reset action when payload has no fileIndexItems", async () => {
    const dispatch = jest.fn();
    const propsHandleExit = jest.fn();

    const mediaArchiveData = {
      data: {
        pageType: PageType.Archive
      },
      statusCode: 200
    };
    const mockFetchGet = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => Promise.resolve(mediaArchiveData))
      .mockImplementationOnce(() => Promise.resolve(mediaArchiveData));

    jest.useFakeTimers();

    RemoveCache(jest.fn(), "/parent", "search", dispatch, propsHandleExit);

    jest.advanceTimersByTime(600);

    await waitFor(() => {
      expect(propsHandleExit).toHaveBeenCalled();

      expect(dispatch).toHaveBeenCalledTimes(0);
    });

    mockFetchGet.mockRestore();
  });
});
