import { act } from "react-dom/test-utils";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import { FileListCache } from "../shared/filelist-cache";
import { mountReactHook } from "./___tests___/test-hook";
import useFileList, {
  IFileList,
  fetchContentUseFileList
} from "./use-filelist";

describe("UseFileList", () => {
  describe("Archive", () => {
    let fetchSpy: jest.SpyInstance<any>;

    function setFetchSpy(statusCode: number, pageType: PageType) {
      const mockSuccessResponse = {
        ...newIArchive(),
        pageType: pageType,
        fileIndexItem: newIFileIndexItem(),
        fileIndexItems: newIFileIndexItemArray()
      };
      const mockJsonPromise = Promise.resolve(mockSuccessResponse); // 2
      const mockResult = Promise.resolve({
        json: () => {
          return mockJsonPromise;
        },
        status: statusCode
      } as Response);

      fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
        return mockResult;
      });
    }

    function mounter() {
      const setupComponent = mountReactHook(useFileList, ["/default/", "1"]); // Mount a Component with our hook
      const hook = setupComponent.componentHook as IFileList;
      return {
        hook,
        setupComponent
      };
    }

    it("with archive content 200", async () => {
      const hook = jest.fn();
      const hook2 = jest.fn();

      const controller = new AbortController();

      setFetchSpy(200, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          hook2,
          false,
          hook
        );
      });

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook).toBeCalledTimes(0);
      expect(hook2).toBeCalledTimes(1);
    });

    it("with detailview content 200", async () => {
      const hook = jest.fn();
      const hook2 = jest.fn();

      const controller = new AbortController();

      setFetchSpy(200, PageType.DetailView);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          hook2,
          false,
          hook
        );
      });

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook).toBeCalledTimes(0);
      expect(hook2).toBeCalledTimes(1);
    });

    it("with archive content 404", async () => {
      const hook = jest.fn();

      const controller = new AbortController();

      setFetchSpy(404, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          jest.fn(),
          false,
          hook
        );
      });

      expect(hook).toBeCalledWith(PageType.NotFound);
    });

    it("with archive content 401", async () => {
      const hook = jest.fn();

      const controller = new AbortController();

      setFetchSpy(401, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          jest.fn(),
          false,
          hook
        );
      });

      expect(hook).toBeCalledWith(PageType.Unauthorized);
    });

    it("with archive content 500", async () => {
      const hook = jest.fn();
      const hook2 = jest.fn();

      const controller = new AbortController();

      setFetchSpy(500, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          hook2,
          false,
          hook
        );
      });

      expect(hook).toBeCalledTimes(1);
      expect(hook2).toBeCalledTimes(0);
    });

    it("get from cache", async () => {
      const { hook } = mounter();
      const cacheGetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockImplementationOnce(() => {
          return { ...newIArchive(), dateCache: Date.now() };
        });

      hook.fetchUseFileListContentCache(
        "location",
        "location",
        new AbortController(),
        jest.fn(),
        false,
        jest.fn()
      );
      expect(cacheGetSpy).toBeCalled();
    });

    it("check cache first and then query", async () => {
      const { hook } = mounter();
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockImplementationOnce(() => {
          return null;
        });

      setFetchSpy(200, PageType.Archive);

      hook.fetchUseFileListContentCache(
        "location",
        "location",
        new AbortController(),
        jest.fn(),
        false,
        jest.fn()
      );

      expect(cacheSetSpy).toBeCalled();
      expect(fetchSpy).toBeCalled();
    });

    it("[use file list] with connection rejected", async () => {
      const { hook } = mounter();

      const controller = new AbortController();

      const mockResult = Promise.reject();

      fetchSpy = jest.spyOn(window, "fetch").mockReset()
      .mockImplementationOnce(() => {
        return mockResult;
      });

      // console.error == undefined
      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          jest.fn(),
          false,
          jest.fn()
        );
      });

      expect(hook.pageType).toBe(PageType.ApplicationException);
    });
  });
});

describe("UseFileList error", () => {
  it("aborted should not call", async () => {
    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new DOMException("aborted");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContentUseFileList(
      "test",
      "test",
      controller,
      jest.fn(),
      false,
      setDataSpy
    );

    // fetchSpy
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    expect(setDataSpy).toBeCalledTimes(0);
  });

  it("generic error", async () => {
    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new Error("default error");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContentUseFileList(
      "test",
      "test",
      controller,
      jest.fn(),
      false,
      setDataSpy
    );
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    expect(setDataSpy).toBeCalled();
  });
});
