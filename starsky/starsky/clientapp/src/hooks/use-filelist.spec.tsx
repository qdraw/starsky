import { act } from "react-dom/test-utils";
import { IArchive, newIArchive } from "../interfaces/IArchive";
import { IRelativeObjects, PageType } from "../interfaces/IDetailView";
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
import * as fetchUseFileListContentCache from "./use-filelist";
import { render } from "@testing-library/react";

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

    const TestComponent = () => {
      useFileList("mockLocationSearch", true);

      // You can use the result object to interact with the state or render UI components

      return null;
    };

    xit("should call setArchive when pageType is Archive", async () => {
      // Mock fetchUseFileListContentCache to resolve immediately
      const fetchSpy = jest
        .spyOn(fetchUseFileListContentCache, "fetchUseFileListContentCache")
        .mockResolvedValue();

      // Render the component
      let result: any;
      await act(async () => {
        result = render(<TestComponent />);
      });

      // Trigger the setPageTypeHelper with a response object having pageType: Archive
      const responseObject = {
        pageType: "Archive",
        data: {
          fileIndexItems: [],
          pageType: "Archive",
          subPath: "/test",
          breadcrumb: [],
          colorClassUsage: [],
          collections: true,
          lastEdited: "",
          lastEditedUtc: "",
          parentDirectory: "",
          relativeObjects: {} as IRelativeObjects,
          colorClassActiveList: [],
          collectionsCount: 0,
          isReadOnly: false,
          dateCache: Date.now()
        } as IArchive
      };
      await act(async () => {
        result.current.setPageTypeHelper(responseObject);
      });

      // Assert that setArchive has been called with the expected data
      expect(result.current).toBe(responseObject.data as unknown as IArchive);

      // Clean up the spies
      fetchSpy.mockRestore();
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
        .mockReset()
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
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockReset()
        .mockImplementationOnce(() => {
          return null;
        });

      const { hook } = mounter();

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
      console.log("[use file list] with connection rejected");

      const controller = new AbortController();

      const mockResult = Promise.reject();

      fetchSpy = jest
        .spyOn(window, "fetch")
        .mockReset()
        .mockImplementationOnce(() => mockResult);

      const setPageTypeFn = jest.fn();

      // console.error == undefined
      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList(
          "test",
          "test",
          controller,
          jest.fn(),
          false,
          setPageTypeFn
        );
      });

      expect(setPageTypeFn).toBeCalledWith(PageType.ApplicationException);
    });
  });
});

describe("UseFileList error", () => {
  it("aborted should not call", async () => {
    const fetchSpy = jest
      .spyOn(window, "fetch")
      .mockReset()
      .mockImplementationOnce(() => {
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
    const fetchSpy = jest
      .spyOn(window, "fetch")
      .mockReset()
      .mockImplementationOnce(() => {
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
