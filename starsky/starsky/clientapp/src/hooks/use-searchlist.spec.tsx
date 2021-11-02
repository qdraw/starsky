import { act } from "react-dom/test-utils";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import useSearchList, { ISearchList } from "./use-searchlist";
import { mountReactHook } from "./___tests___/test-hook";

describe("UseSearchList", () => {
  describe("Search", () => {
    let setupComponent;
    let hook: ISearchList;

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

    beforeEach(() => {
      setupComponent = mountReactHook(useSearchList, ["test", "1", "true"]); // Mount a Component with our hook
      hook = setupComponent.componentHook as ISearchList;
    });

    it("with search content 200", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      var controller = new AbortController();

      setFetchSpy(200, PageType.Search);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      if (!hook.archive) throw Error("missing archive");

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "GET",
        signal: controller.signal
      });

      expect(hook.archive.fileIndexItems).toStrictEqual([]);
    });

    it("with search content 404", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      var controller = new AbortController();

      setFetchSpy(404, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.NotFound);
    });

    it("with search content 401", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      var controller = new AbortController();

      setFetchSpy(401, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.Unauthorized);
    });

    it("with search content 500", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      var controller = new AbortController();

      setFetchSpy(500, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContent("test", controller);
      });

      expect(hook.pageType).toBe(PageType.ApplicationException);
    });
  });
});
