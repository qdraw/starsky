import { mount, shallow } from 'enzyme';
import React from 'react';
import CurrentLocationButton from './current-location-button';

describe("CurrentLocationButton", () => {

  it("renders", () => {
    shallow(<CurrentLocationButton />)
  });

  describe("context", () => {

    it("renders", () => {


      var component = mount(<CurrentLocationButton />);
      component.find('button').simulate('click');
    });

  });
});