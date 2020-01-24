import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import React from 'react';
import MoreMenu, { MoreMenuEventCloseConst } from './more-menu';

describe("More Menu", () => {
  it("renders", () => {
    shallow(<MoreMenu />)
  });

  it("get childeren", () => {
    var element = shallow(<MoreMenu>
      test
    </MoreMenu>);
    expect(element.find(".menu-options").text()).toBe('test')
  });

  it("toggle", () => {
    var element = shallow(<MoreMenu>
      test
    </MoreMenu>);

    act(() => {
      element.find(".menu-context").simulate('click');
    })

    expect(element.find(".menu-context").props().className).toBe('menu-context')
  });

  it("toggle no childeren", () => {
    var element = shallow(<MoreMenu />);

    act(() => {
      element.find(".menu-context").simulate('click');
    })

    expect(element.find(".menu-context").props().className).toBe('menu-context menu-context--hide')
  });

  xit("turn off using event", (done) => {
    var element = mount(<MoreMenu>
      test
    </MoreMenu>);

    act(() => {
      element.find(".menu-context").simulate('click');
    })

    window.addEventListener(MoreMenuEventCloseConst, () => {
      expect(element.find(".menu-context").props().className).toBe('menu-context menu-context--hide')
      done();
    });

    act(() => {
      window.dispatchEvent(new CustomEvent(MoreMenuEventCloseConst));
    })



  });


});
