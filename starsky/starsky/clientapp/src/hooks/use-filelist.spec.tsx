import React, { act } from "react";
import { IArchive, newIArchive } from "../interfaces/IArchive";
import { IDetailView, IRelativeObjects, PageType } from "../interfaces/IDetailView";
import { newIFileIndexItem, newIFileIndexItemArray } from "../interfaces/IFileIndexItem";
import { FileListCache } from "../shared/filelist-cache";
import { mountReactHook } from "./___tests___/test-hook";
import useFileList, { IFileList, fetchContentUseFileList } from "./use-filelist";

describe("UseFileList", () => {
  describe("Archive", () => {
    let fetchSpy: jest.SpyInstance;

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

    function mounter(path: string = "/default/") {
      const setupComponent = mountReactHook(useFileList as (...args: unknown[]) => unknown, [
        path,
        "1"
      ]);
      // Mount a Component with our hook
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
        await fetchContentUseFileList("test", "test", controller, hook2, false, hook);
      });

      expect(fetchSpy).toHaveBeenCalled();
      expect(fetchSpy).toHaveBeenCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook).toHaveBeenCalledTimes(0);
      expect(hook2).toHaveBeenCalledTimes(1);
    });

    it("with detailView content 200", async () => {
      const hook = jest.fn();
      const hook2 = jest.fn();

      const controller = new AbortController();

      setFetchSpy(200, PageType.DetailView);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList("test", "test", controller, hook2, false, hook);
      });

      expect(fetchSpy).toHaveBeenCalled();
      expect(fetchSpy).toHaveBeenCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook).toHaveBeenCalledTimes(0);
      expect(hook2).toHaveBeenCalledTimes(1);
    });

    it("with archive content 404", async () => {
      const hook = jest.fn();

      const controller = new AbortController();

      setFetchSpy(404, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList("test", "test", controller, jest.fn(), false, hook);
      });

      expect(hook).toHaveBeenCalledWith(PageType.NotFound);
    });

    it("with archive content 401", async () => {
      const hook = jest.fn();

      const controller = new AbortController();

      setFetchSpy(401, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList("test", "test", controller, jest.fn(), false, hook);
      });

      expect(hook).toHaveBeenCalledWith(PageType.Unauthorized);
    });

    it("with archive content 500", async () => {
      const hook = jest.fn();
      const hook2 = jest.fn();

      const controller = new AbortController();

      setFetchSpy(500, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await fetchContentUseFileList("test", "test", controller, hook2, false, hook);
      });

      expect(hook).toHaveBeenCalledTimes(1);
      expect(hook2).toHaveBeenCalledTimes(0);
    });

    it("get from cache", async () => {
      const { hook } = mounter();
      const cacheGetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockReset()
        .mockImplementationOnce(() => {
          return { ...newIArchive(), dateCache: Date.now() };
        });

      await hook.fetchUseFileListContentCache(
        "location",
        "location",
        new AbortController(),
        jest.fn(),
        false,
        jest.fn()
      );
      expect(cacheGetSpy).toHaveBeenCalled();
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

      await hook.fetchUseFileListContentCache(
        "location",
        "location",
        new AbortController(),
        jest.fn(),
        false,
        jest.fn()
      );

      expect(cacheSetSpy).toHaveBeenCalled();
      expect(fetchSpy).toHaveBeenCalled();
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
        await fetchContentUseFileList("test", "test", controller, jest.fn(), false, setPageTypeFn);
      });

      expect(setPageTypeFn).toHaveBeenCalledWith(PageType.ApplicationException);
    });

    it("setPageTypeHelper - undefined false", () => {
      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper(undefined as unknown as IDetailView);
      expect(pageHelper).toBeFalsy();
    });

    it("setPageTypeHelper - pageType not found false", () => {
      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper({ pageType: "NotFound" } as unknown as IDetailView);
      expect(pageHelper).toBeFalsy();
    });

    it("setPageTypeHelper - pageType ApplicationException false", () => {
      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper({
        pageType: "ApplicationException"
      } as unknown as IDetailView);
      expect(pageHelper).toBeFalsy();
    });

    it("setPageTypeHelper - pageType DifferentType false", () => {
      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper({
        pageType: "DifferentType"
      } as unknown as IDetailView);
      expect(pageHelper).toBeFalsy();
    });

    it("setPageTypeHelper - pageType Archive", () => {
      const useStateMock = jest.fn();
      jest.spyOn(React, "useState").mockImplementationOnce(() => {
        return [true, useStateMock];
      });

      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper({
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
      } as unknown as IArchive);

      expect(pageHelper).toBeTruthy();
      expect(useStateMock).toHaveBeenCalled();
      expect(useStateMock).toHaveBeenCalledWith({
        data: {
          breadcrumb: [],
          collections: true,
          collectionsCount: 0,
          colorClassActiveList: [],
          colorClassUsage: [],
          dateCache: expect.any(Number),
          fileIndexItems: [],
          isReadOnly: false,
          lastEdited: "",
          lastEditedUtc: "",
          pageType: "Archive",
          parentDirectory: "",
          relativeObjects: {},
          subPath: "/test"
        },
        pageType: "Archive",
        sort: undefined
      });
    });

    it("setPageTypeHelper - pageType DetailView", () => {
      const useStateMock = jest.fn();
      jest
        .spyOn(React, "useState")
        .mockImplementationOnce(() => {
          return [true, jest.fn()];
        })
        .mockImplementationOnce(() => {
          return [true, useStateMock];
        });

      const { hook } = mounter("/test.jpg");

      const pageHelper = hook.setPageTypeHelper({
        pageType: "DetailView",
        data: {
          pageType: PageType.DetailView,
          subPath: "/test.jpg"
        } as IDetailView
      } as unknown as IDetailView);

      expect(pageHelper).toBeTruthy();
      expect(useStateMock).toHaveBeenCalled();
      expect(useStateMock).toHaveBeenCalledWith({
        data: {
          pageType: "DetailView",
          subPath: "/test.jpg"
        },
        pageType: "DetailView",
        sort: undefined
      });
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
    await fetchContentUseFileList("test", "test", controller, jest.fn(), false, setDataSpy);

    // fetchSpy
    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    expect(setDataSpy).toHaveBeenCalledTimes(0);
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
    await fetchContentUseFileList("test", "test", controller, jest.fn(), false, setDataSpy);
    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    expect(setDataSpy).toHaveBeenCalled();
  });
});
