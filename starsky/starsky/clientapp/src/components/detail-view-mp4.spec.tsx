import { mount, shallow } from 'enzyme';
import React from 'react';
import DetailViewMp4 from './detail-view-mp4';

describe("DetailViewGpx", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewMp4></DetailViewMp4>)
  });

  describe("with Context", () => {
    it("renders with example GPX", () => {
      var component = mount(<DetailViewMp4></DetailViewMp4>);

      // need to await before the maps are added
      component.find('[data-test="video"]').simulate("click");
    });
  });
});