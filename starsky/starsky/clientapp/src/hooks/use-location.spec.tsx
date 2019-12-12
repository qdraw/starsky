import { mount } from 'enzyme';
import React from 'react';
import useLocation from './use-location';


describe("useLocation", () => {

  const UseLocationComponentTest: React.FunctionComponent<any> = () => {
    useLocation();
    return null;
  };

  it("check if is called once", () => {
    const setState = jest.fn();
    const useStateSpy = jest.spyOn(React, 'useState').mockImplementationOnce(() => {
      return [setState, setState]
    })

    mount(<UseLocationComponentTest></UseLocationComponentTest>);

    expect(useStateSpy).toBeCalled();
  });

});