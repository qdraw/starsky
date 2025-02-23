import { newIArchive } from "../interfaces/IArchive";
import { IConnectionDefault } from "../interfaces/IConnectionDefault";
import { PageType } from "../interfaces/IDetailView";
import { newIFileIndexItemArray } from "../interfaces/IFileIndexItem";
import { mountReactHook } from "./___tests___/test-hook";
import useFetch, { fetchContent } from "./use-fetch";

describe("UseFetch", () => {
  let setupComponent;
  let hook: IConnectionDefault;

  let fetchSpy: jest.SpyInstance<Promise<Response>>;

  function setFetchSpy(statusCode: number) {
    const mockSuccessResponse = {
      ...newIArchive(),
      pageType: PageType.Archive,
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
    setupComponent = mountReactHook(useFetch as (...args: unknown[]) => unknown, [
      "/default/",
      "get"
    ]); // Mount a Component with our hook
    hook = setupComponent.componentHook as IConnectionDefault;
  });

  it("default status code", () => {
    expect(hook.statusCode).toBe(999);
  });

  it("with default archive feedback", async () => {
    setFetchSpy(200);
    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContent("test", "get", true, controller, setDataSpy);

    // fetchSpy
    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    // setData
    expect(setDataSpy).toHaveBeenCalled();
    expect(setDataSpy).toHaveBeenCalledWith({
      data: { fileIndexItems: [], pageType: "Archive" },
      statusCode: 200
    });
  });
});

describe("UseFetch error", () => {
  it("aborted", async () => {
    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new Error("aborted");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContent("test", "get", true, controller, setDataSpy);

    // fetchSpy
    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    expect(setDataSpy).toHaveBeenCalledTimes(0);
  });

  it("non aborted", async () => {
    const fetchSpy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new Error("default error");
    });

    const controller = new AbortController();
    const setDataSpy = jest.fn();
    await fetchContent("test", "get", true, controller, setDataSpy);

    // fetchSpy
    expect(fetchSpy).toHaveBeenCalled();
    expect(fetchSpy).toHaveBeenCalledWith("test", {
      credentials: "include",
      method: "get",
      signal: controller.signal
    });

    // setData
    expect(setDataSpy).toHaveBeenCalled();
    expect(setDataSpy).toHaveBeenCalledWith({
      data: null,
      statusCode: 999
    });
  });
});
