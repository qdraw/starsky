import { mount } from 'enzyme';
import React from 'react';
import { TestHook } from './test-hook';
import useFileList from './use-filelist';


describe("UseFileList", () => {

  const testHook = (callback: any) => {
    return mount(<TestHook callback={callback} />);
  };

  it("call api", () => {
    const setState = jest.fn();
    const useStateSpy = jest.spyOn(React, 'useState').mockImplementationOnce(() => {
      return [setState, setState]
    });

    jest.spyOn(window, 'fetch').mockImplementationOnce(() => {
      return Promise.resolve(
        {
          json: () => { },
          status: 200
        } as Response,
      )
    });

    testHook(useFileList);

    expect(fetch).toHaveBeenCalled()
    expect(useStateSpy).toHaveBeenCalled()
  });


});