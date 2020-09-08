import { shallow } from 'enzyme';
import React from 'react';
import HamburgerMenuToggle from './hamburger-menu-toggle';


describe("HamburgerMenuToggle", () => {

  it("renders", () => {
    shallow(<HamburgerMenuToggle select={false} hamburgerMenu={true} setHamburgerMenu={jest.fn()} />)
  });

});
