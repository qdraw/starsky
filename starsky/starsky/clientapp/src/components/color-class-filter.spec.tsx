import { shallow } from 'enzyme';
import React from 'react';
import ColorClassFilter from './color-class-filter';

describe("ColorClassFilter", () => {

  it("renders", () => {
    shallow(<ColorClassFilter itemsCount={1} subPath={"/test"} colorClassActiveList={[1, 2]} colorClassUsage={[1, 2]}></ColorClassFilter>)
  });

  it("onClick value", () => {

    var component = shallow(<ColorClassFilter itemsCount={1} subPath={"/test"} colorClassActiveList={[1]} colorClassUsage={[1, 2]}></ColorClassFilter>)

    component.find('.colorclass--2').last().simulate('click');
  });


  it("onClick disabled", () => {
    var component = shallow(<ColorClassFilter itemsCount={0} subPath={"/test"} colorClassActiveList={[1]} colorClassUsage={[1, 2]}></ColorClassFilter>)


  });
});
