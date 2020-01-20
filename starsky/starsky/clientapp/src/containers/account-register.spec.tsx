import { mount, shallow } from "enzyme";
import React from 'react';
import AccountRegister from './account-register';

describe("AccountRegister", () => {
  it("renders", () => {
    shallow(<AccountRegister />)
  });

  it("no colorclass usage", () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
      return newIConnectionDefault();
    })

    var container = mount(<AccountRegister />);
    // const container = shallow(<Archive {...newIArchive()} />);
    // expect(container.text()).toBe('(Archive) => no colorClassLists')
  });

  it("check if warning exist with no items in the list", () => {

    // jest.spyOn(window, 'scrollTo')
    //   .mockImplementationOnce(() => { });

    // const container = mount(<Archive {...newIArchive()}
    //   colorClassFilterList={[]}
    //   colorClassUsage={[]}
    //   fileIndexItems={[]} />);
    // expect(container.exists('.warning-box')).toBeTruthy()
  });
});