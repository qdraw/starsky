import { mount, shallow } from 'enzyme';
import React from 'react';
import MenuArchive from './menu-archive';

describe("MenuArchive", () => {

  it("renders", () => {
    shallow(<MenuArchive />)
  });

  describe("with Context", () => {
    it("default", () => {
      var component = mount(<MenuArchive />);
      expect(component.exists('[data-test="hamburger"]')).toBeTruthy();
      console.log(component.html());

    });

  });

});

