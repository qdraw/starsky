import { shallow } from "enzyme";
import React from 'react';
import Breadcrumb from './breadcrumbs';


describe("Breadcrumb", () => {
  it("renders", () => {
    shallow(<Breadcrumb subPath="/" breadcrumb={["/"]} />)
  });

  it("disabled", () => {
    var wrapper = shallow(<Breadcrumb subPath="" breadcrumb={[]} />)
    expect(wrapper.find('span')).toHaveLength(0);
  });

  it("check lenght for breadcrumbs", () => {
    var breadcrumbs = ["/", "/test"]
    var wrapper = shallow(<Breadcrumb subPath="/test" breadcrumb={breadcrumbs} />)
    expect(wrapper.find('span')).toHaveLength(3);
  });

});