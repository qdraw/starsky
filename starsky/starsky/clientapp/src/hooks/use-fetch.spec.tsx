import { newIArchive } from '../interfaces/IArchive';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { PageType } from '../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import useFetch, { fetchContent } from './use-fetch';
import { mountReactHook } from './___tests___/test-hook';


describe("UseFetch", () => {
  let setupComponent;
  let hook: IConnectionDefault;

  let fetchSpy: jest.SpyInstance<any>;

  function setFetchSpy(statusCode: number) {
    const mockSuccessResponse = { ...newIArchive(), pageType: PageType.Archive, fileIndexItems: newIFileIndexItemArray() };
    const mockJsonPromise = Promise.resolve(mockSuccessResponse); // 2
    const mockResult = Promise.resolve(
      {
        json: () => {
          return mockJsonPromise;
        },
        status: statusCode
      } as Response,
    );

    fetchSpy = jest.spyOn(window, 'fetch').mockImplementationOnce(() => {
      return mockResult;
    });
  }

  beforeEach(() => {
    setupComponent = mountReactHook(useFetch, ["/default/", "get"]); // Mount a Component with our hook
    hook = setupComponent.componentHook as IConnectionDefault;
  });

  it('default status code', () => {
    expect(hook.statusCode).toBe(999)
  })

  it('with default archive feedback', async () => {
    setFetchSpy(200);
    var controller = new AbortController();
    var setDataSpy = jest.fn()
    await fetchContent("test", 'get', true, controller, setDataSpy);

    // fetchSpy
    expect(fetchSpy).toBeCalled()
    expect(fetchSpy).toBeCalledWith('test', { "credentials": "include", "method": "get", "signal": controller.signal });

    // setData
    expect(setDataSpy).toBeCalled()
    expect(setDataSpy).toBeCalledWith({ "data": { "fileIndexItems": [], "pageType": "Archive" }, "statusCode": 200 })
  })

});