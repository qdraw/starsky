import { mount, shallow } from 'enzyme';
import React from 'react';
import { IRelativeObjects, newIRelativeObjects } from '../interfaces/IDetailView';
import RelativeLink from './relative-link';

describe("RelativeLink", () => {
  it("renders", () => {
    shallow(<RelativeLink relativeObjects={newIRelativeObjects()}></RelativeLink>)
  });

  var relativeObjects = { nextFilePath: "next", prevFilePath: 'prev' } as IRelativeObjects;
  var Component = mount(<RelativeLink relativeObjects={relativeObjects}></RelativeLink>)

  it("next page exist", () => {
    expect(Component.find('a.next').props().href).toBe('/?f=next')
  });

  it("prev page exist", () => {
    expect(Component.find('a.prev').props().href).toBe('/?f=prev')
  });


});
