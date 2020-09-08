import { shallow } from 'enzyme';
import React from 'react';
import NavContainer from './nav-container';

describe("NavContainer", () => {

  it("renders", () => {
    shallow(<NavContainer hamburgerMenu={true}>
      content
    </NavContainer>)
  });

});
