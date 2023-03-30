import { act } from "react-dom/test-utils";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import useSearchList, {
  fetchContentUseSearchList,
  ISearchList
} from "./use-searchlist";
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

      const controller = new AbortController();
      const setArchiveSpy = jest.fn();

      setFetchSpy(200, PageType.Search);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContentUseSearchList(
          "test",
          controller,
          setArchiveSpy,
          jest.fn(),
          false
        );
      });

      if (!hook.archive) throw Error("missing archive");

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith("test", {
        credentials: "include",
        method: "GET",
        signal: controller.signal
      });

      expect(setArchiveSpy).toBeCalledWith({
        colorClassActiveList: [],
        colorClassUsage: [],
        fileIndexItem: { description: "", tags: "", title: "" },
        fileIndexItems: [],
        pageType: "Search"
      });
    });

    it("with search content 404", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      const controller = new AbortController();
      const setPageTypeSpy = jest.fn();

      setFetchSpy(404, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContentUseSearchList(
          "test",
          controller,
          jest.fn(),
          setPageTypeSpy,
          false
        );
      });

      expect(setPageTypeSpy).toBeCalledWith(PageType.NotFound);
    });

    it("with search content 401", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      const controller = new AbortController();
      const setPageTypeSpy = jest.fn();

      setFetchSpy(401, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContentUseSearchList(
          "test",
          controller,
          jest.fn(),
          setPageTypeSpy,
          false
        );
      });

      expect(setPageTypeSpy).toBeCalledWith(PageType.Unauthorized);
    });

    it("with search content 500", async () => {
      expect(hook.pageType).toBe(PageType.Loading);

      const controller = new AbortController();
      const setPageTypeSpy = jest.fn();

      setFetchSpy(500, PageType.Archive);

      await act(async () => {
        // perform changes within our component
        await hook.fetchContentUseSearchList(
          "test",
          controller,
          jest.fn(),
          setPageTypeSpy,
          false
        );
      });
      expect(setPageTypeSpy).toBeCalledWith(PageType.ApplicationException);
    });
  });
});

describe("UseSearchList error", () => {
  it("aborted should not call", async () => {
    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new DOMException("aborted");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContentUseSearchList(
      "test",
      controller,
      jest.fn(),
      setDataSpy,
      false
    );

    // fetchSpy
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith("test", {
      credentials: "include",
      method: "GET",
      signal: controller.signal
    });

    expect(setDataSpy).toBeCalledTimes(0);
  });

  it("generic error", async () => {
    console.log("generic error");

    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new Error("default error");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContentUseSearchList(
      "test",
      controller,
      jest.fn(),
      setDataSpy,
      false
    );
    expect(fetchSpy).toBeCalled();
    expect(fetchSpy).toBeCalledWith("test", {
      credentials: "include",
      method: "GET",
      signal: controller.signal
    });

    expect(setDataSpy).toBeCalledTimes(0);
  });
});
