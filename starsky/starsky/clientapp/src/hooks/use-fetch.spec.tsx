import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import useFetch, { fetchContent } from './use-fetch';
import { mountReactHook } from './___tests___/test-hook';


describe("UseFetch", () => {
  let setupComponent;
  let hook: IConnectionDefault;

  beforeEach(() => {
    setupComponent = mountReactHook(useFetch, ["/default/", "get"]); // Mount a Component with our hook
    hook = setupComponent.componentHook as IConnectionDefault;
  });


  it('default status code', () => {
    expect(hook.statusCode).toBe(999)
  })

  it('dd', async () => {

    var controller = new AbortController();
    var content = await fetchContent("t", 'get', true, controller, jest.fn())
    console.log(content);

  })


  // const testHook = (callback: any) => {
  //   return mount(<TestHook callback={callback} />);
  // };

  // it("call api", () => {
  //   const setState = jest.fn();
  //   const useStateSpy = jest.spyOn(React, 'useState').mockImplementationOnce(() => {
  //     return [setState, setState]
  //   });

  //   var fetch = jest.spyOn(window, 'fetch').mockImplementationOnce(() => {
  //     return Promise.resolve(
  //       {
  //         json: () => { },
  //         status: 200
  //       } as Response,
  //     )
  //   });

  //   testHook(useFetch);

  //   expect(fetch).toHaveBeenCalled()
  //   expect(useStateSpy).toHaveBeenCalled()

  // });
});