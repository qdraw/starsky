import { shallow } from 'enzyme';
import React from 'react';
import MoreMenu from './more-menu';
describe("More Menu", () => {

  it("renders", () => {
    shallow(<MoreMenu />)
  });

  it("get childeren", () => {
    var element = shallow(<MoreMenu>
      test
    </MoreMenu>)
    expect(element.find(".menu-options").text()).toBe('test')
  });

  it("toggle", () => {
    var element = shallow(<MoreMenu>
      test
    </MoreMenu>)
    element.find(".menu-context").simulate('click');
    expect(element.find(".menu-context").props().className).toBe('menu-context')
  });

  it("toggle no childeren", () => {
    var element = shallow(<MoreMenu></MoreMenu>)
    element.find(".menu-context").simulate('click');
    expect(element.find(".menu-context").props().className).toBe('menu-context menu-context--hide')
  });

});