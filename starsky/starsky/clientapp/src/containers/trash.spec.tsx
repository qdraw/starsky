import { mount, shallow } from "enzyme";
import React from 'react';
import * as useFetch from '../hooks/use-fetch';
import { newIArchive } from '../interfaces/IArchive';
import { newIConnectionDefault } from '../interfaces/IConnectionDefault';
import Trash from './trash';

describe("Trash", () => {
  it("renders", () => {
    shallow(<Trash {...newIArchive()} />)
  });

  it("check if warning exist with no items in the list", () => {

    // usage ==> import * as useFetch from '../hooks/use-fetch';
    var spyGet = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
      return newIConnectionDefault();
    });

    jest.spyOn(window, 'scrollTo')
      .mockImplementationOnce(() => { });

    const container = mount(<Trash {...newIArchive()}
      colorClassActiveList={[]}
      colorClassUsage={[]}
      fileIndexItems={[]} />);
    expect(container.exists('.warning-box')).toBeTruthy()

    expect(spyGet).toBeCalled();
  });

});