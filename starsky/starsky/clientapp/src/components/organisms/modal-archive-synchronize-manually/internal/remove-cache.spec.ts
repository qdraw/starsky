import { FileListCache } from "../../../../shared/filelist-cache";
import { RemoveCache } from "./remove-cache";

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
    const fetchGet = jest.spyOn(global, "FetchGet").mockResolvedValue();
    const parentFolder = "/parent";
    RemoveCache(jest.fn(), parentFolder, "search", jest.fn(), jest.fn());
    expect(fetchGet).toHaveBeenCalledWith(expect.any(String));
    expect(fetchGet.mock.calls[0][0]).toContain("/remove-cache");
    expect(fetchGet.mock.calls[0][0]).toContain(encodeURIComponent(parentFolder));
    fetchGet.mockRestore();
  });

  it("should call FetchGet with the correct URL to index server API after setTimeout", () => {
    const fetchGet = jest.spyOn(global, "FetchGet").mockResolvedValue();
    const setTimeoutSpy = jest.spyOn(global, "setTimeout");
    const parentFolder = "/parent";
    const historyLocationSearch = "search";
    RemoveCache(jest.fn(), parentFolder, historyLocationSearch, jest.fn(), jest.fn());

    expect(setTimeoutSpy).toHaveBeenCalledWith(expect.any(Function), 600);
    jest.runAllTimers();

    expect(fetchGet).toHaveBeenCalledWith(expect.any(String));
    expect(fetchGet.mock.calls[1][0]).toContain("/index-server-api");
    expect(fetchGet.mock.calls[1][0]).toContain(historyLocationSearch);

    fetchGet.mockRestore();
    setTimeoutSpy.mockRestore();
  });

  it("should dispatch force-reset action when payload has fileIndexItems", async () => {
    const dispatch = jest.fn();
    const mediaArchiveData = {
      data: {
        fileIndexItems: [{}, {}] // Mocking fileIndexItems
      }
    };
    const fetchGet = jest.spyOn(global, "FetchGet").mockResolvedValue(mediaArchiveData);
    await RemoveCache(jest.fn(), "/parent", "search", dispatch, jest.fn());

    expect(dispatch).toHaveBeenCalledWith({ type: "force-reset", payload: mediaArchiveData.data });
    fetchGet.mockRestore();
  });

  it("should call propsHandleExit after completing", async () => {
    const propsHandleExit = jest.fn();
    const dispatch = jest.fn();
    await RemoveCache(jest.fn(), "/parent", "search", dispatch, propsHandleExit);
    expect(propsHandleExit).toHaveBeenCalled();
  });
});
