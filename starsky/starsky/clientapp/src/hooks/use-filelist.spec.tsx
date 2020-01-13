import { act } from 'react-dom/test-utils';
import { newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import useFileList, { IFileList } from './use-filelist';
import { mountReactHook } from './___tests___/test-hook';


describe("UseFileList", () => {

  describe("Archive", () => {
    let setupComponent;
    let hook: IFileList;

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
      setupComponent = mountReactHook(useFileList, ["/default/", "1"]); // Mount a Component with our hook
      hook = setupComponent.componentHook as IFileList;
    });

    it("with archive content 200", async () => {
      expect(hook.pageType).toBe(PageType.Loading)

      var controller = new AbortController();

      setFetchSpy(200);

      await act(async () => { // perform changes within our component
        await hook.fetchContent('test', controller);
      });

      if (!hook.archive) throw Error('missing archive');

      expect(fetchSpy).toBeCalled();
      expect(fetchSpy).toBeCalledWith('test', { "credentials": "include", "method": "get", "signal": controller.signal });

      expect(hook.archive.fileIndexItems).toStrictEqual([])
    });

    it("with archive content 404", async () => {
      expect(hook.pageType).toBe(PageType.Loading)

      var controller = new AbortController();

      setFetchSpy(404);

      await act(async () => { // perform changes within our component
        await hook.fetchContent('test', controller);
      });

      expect(hook.pageType).toBe(PageType.NotFound)

    });

    it("with archive content 401", async () => {
      expect(hook.pageType).toBe(PageType.Loading)

      var controller = new AbortController();

      setFetchSpy(401);

      await act(async () => { // perform changes within our component
        await hook.fetchContent('test', controller);
      });

      expect(hook.pageType).toBe(PageType.Unauthorized)

    });

    it("with archive content 500", async () => {
      expect(hook.pageType).toBe(PageType.Loading)

      var controller = new AbortController();

      setFetchSpy(500);

      await act(async () => { // perform changes within our component
        await hook.fetchContent('test', controller);
      });

      expect(hook.pageType).toBe(PageType.ApplicationException)

    });

  });

});