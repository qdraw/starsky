import { mount, shallow } from "enzyme";
import React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import Archive from './archive';

describe("Archive", () => {
  it("renders", () => {
    shallow(<Archive {...newIArchive()} />)
  });

  it("no colorclass usage", () => {
    const container = shallow(<Archive {...newIArchive()} />);
    expect(container.text()).toBe('(Archive) => no colorClassLists')
  });

  it("check if warning exist with no items in the list", () => {
    jest.spyOn(window, 'scrollTo')
      .mockImplementationOnce(() => { });

    const container = mount(<Archive {...newIArchive()}
      colorClassActiveList={[]}
      colorClassUsage={[]}
      fileIndexItems={[]} />);
    expect(container.exists('.warning-box')).toBeTruthy()
  });
});