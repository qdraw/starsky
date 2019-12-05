import { mount } from 'enzyme';
import React from 'react';
import { TestHook } from './test-hook';
import useFetch from './use-fetch';


describe("DetailViewWrapper", () => {

  const testHook = (callback: any) => {
    return mount(<TestHook callback={callback} />);
  };

  it("call api", () => {
    const setState = jest.fn();
    const useStateSpy = jest.spyOn(React, 'useState').mockImplementationOnce(() => {
      return [setState, setState]
    })

    jest.spyOn(window, 'fetch').mockImplementationOnce(() => {
      return Promise.resolve(
        {
          json: () => { },
          status: 200
        } as Response,
      )
    })

    testHook(useFetch);

    expect(useStateSpy).toHaveBeenCalled()

  });
});