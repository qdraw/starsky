import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import MenuTrash from './menu-trash';

describe("MenuTrash", () => {

  // todo: fix test

  xit("renders", () => {
    shallow(<MenuTrash defaultText="" callback={() => { }} />)
  });

  describe("with Context", () => {
    xit("open hamburger menu", () => {
      var component = mount(<MenuTrash />)
      var hamburger = component.find('.hamburger');

      expect(component.exists('.form-nav')).toBeTruthy();
      expect(component.exists('.hamburger.open')).toBeFalsy();
      expect(component.exists('.nav.open')).toBeFalsy();

      act(() => {
        hamburger.simulate('click');
      });

      // find does not work
      expect(component.html()).toContain('hamburger open')
      expect(component.html()).toContain('nav open')
      expect(component.exists('.form-nav')).toBeTruthy();

    });
  });
});
