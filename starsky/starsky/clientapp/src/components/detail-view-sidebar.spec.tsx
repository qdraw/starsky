import { shallow } from 'enzyme';
import React from 'react';
import { IExifStatus } from '../interfaces/IExifStatus';
import DetailViewSidebar from './detail-view-sidebar';

describe("DetailViewSidebar", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewSidebar status={IExifStatus.Default}
      filePath={"/t"}>></DetailViewSidebar>)
  });


  it("mount (without state component) and check if there is a warning", () => {
    var wrapper = shallow(<DetailViewSidebar status={IExifStatus.Default} filePath={"/t"}>></DetailViewSidebar>)
    expect(wrapper.find('.sidebar').find('.warning-box')).toHaveLength(1);
  });


});