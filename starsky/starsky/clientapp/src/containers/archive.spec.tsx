import { mount, shallow } from "enzyme";
import React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import Archive from './archive';

describe("Archive", () => {
  it("renders", () => {
    shallow(<Archive {...newIArchive()} />)
  });

  it("no colorclass usage", () => {
    var container = shallow(<Archive {...newIArchive()} />);
    expect(container.text()).toBe('(Archive) => no colorClassLists')
  });

  it("check if warning exist with no items in the list", () => {
    var container = mount(<Archive {...newIArchive()}
      colorClassFilterList={[]}
      colorClassUsage={[]}
      fileIndexItems={[]} />);
    expect(container.exists('.warning-box')).toBeTruthy()
  });
});