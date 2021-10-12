import { act } from "react-dom/test-utils";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import { FileListCache } from "../shared/filelist-cache";
import useFileList, { IFileList } from "./use-filelist";
import { mountReactHook } from "./___tests___/test-hook";

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
      const { hook } = mounter();

      var controller = new AbortController();

      setFetchSpy(200, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      if (!hook.archive) throw Error("missing archive");

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook.archive.fileIndexItems).toStrictEqual([]);
    });

    it("with detailview content 200", async () => {
      const { hook } = mounter();

      var controller = new AbortController();

      setFetchSpy(200, PageType.DetailView);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      if (!hook.detailView) throw Error("missing detailView");

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "get",
        signal: controller.signal
      });

      expect(hook.detailView.fileIndexItem).toStrictEqual(newIFileIndexItem());
    });

    it("with archive content 404", async () => {
      const { hook } = mounter();

      var controller = new AbortController();

      setFetchSpy(404, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.NotFound);
    });

    it("with archive content 401", async () => {
      const { hook } = mounter();

      var controller = new AbortController();

      setFetchSpy(401, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.Unauthorized);
    });

    it("with archive content 500", async () => {
      const { hook } = mounter();

      var controller = new AbortController();

      setFetchSpy(500, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.ApplicationException);
    });

    it("get from cache", async () => {
      const { hook } = mounter();
      var cacheGetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockImplementationOnce(() => {
          return { ...newIArchive(), dateCache: Date.now() };
        });

      hook.fetchContentCache("location", new AbortController());
      expect(cacheGetSpy).toBeCalled();
    });

    it("check cache first and then query", async () => {
      const { hook } = mounter();
      var cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheGet")
        .mockImplementationOnce(() => {
          return null;
        });

      setFetchSpy(200, PageType.Archive);

      hook.fetchContentCache("location", new AbortController());

      expect(cacheSetSpy).toBeCalled();
      expect(fetchSpy).toBeCalled();
    });

    it("with connection rejected", async () => {
      const { hook } = mounter();

      var controller = new AbortController();

      const mockResult = Promise.reject();
      fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
        return mockResult;
      });

      // console.error == undefined
      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.ApplicationException);
    });
  });
});
