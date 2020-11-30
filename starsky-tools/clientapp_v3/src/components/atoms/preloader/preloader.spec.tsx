import { shallow } from 'enzyme';
import React from 'react';
import Preloader from './preloader';

describe("Preloader", () => {

  it("renders", () => {
    shallow(<Preloader isOverlay={false} />)
  });

  it("no overlay", () => {
    var component = shallow(<Preloader isOverlay={false} />);
    expect(component.exists('.preloader--overlay')).toBeFalsy()
  });

  it("with overlay", () => {
    var component = shallow(<Preloader isOverlay={true} />);
    expect(component.exists('.preloader--overlay')).toBeTruthy()
  });
});
