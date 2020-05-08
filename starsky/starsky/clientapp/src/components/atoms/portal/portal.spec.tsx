import { mount, shallow } from 'enzyme';
import React from 'react';
import Portal from './portal';

describe("Portal", () => {

  it("renders", () => {
    shallow(<Portal />)
  });

  it("default render", () => {
    var component = mount(<Portal />);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);
    component.unmount();
  });

  it("default cleanup after render", () => {
    var component = mount(<Portal />);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);
    component.unmount();
    expect(document.querySelectorAll("#portal-root").length).toBe(0);
  });

  it("null cleanup after render", () => {
    var component = mount(<Portal />);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);

    var tempItem = document.querySelector("#portal-root");
    if (!tempItem) throw Error("missing item");
    tempItem.remove();

    component.unmount();
    expect(document.querySelectorAll("#portal-root").length).toBe(0);
  });
});
