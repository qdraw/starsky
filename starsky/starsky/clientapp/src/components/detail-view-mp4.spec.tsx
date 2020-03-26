import { shallow } from 'enzyme';
import React from 'react';
import DetailViewMp4 from './detail-view-mp4';

describe("DetailViewGpx", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewMp4></DetailViewMp4>)
  });
});