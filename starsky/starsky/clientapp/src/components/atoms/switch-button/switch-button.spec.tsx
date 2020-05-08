import { shallow } from 'enzyme';
import React from 'react';
import SwitchButton from './switch-button';

describe("SwitchButton", () => {

  it("renders", () => {
    var toggle = jest.fn();
    shallow(<SwitchButton onToggle={toggle} leftLabel={"on"} rightLabel={"off"}/>)
  });

  it("renders (disabled:state)", () => {
    var toggle = jest.fn();
    var wrapper = shallow(<SwitchButton isEnabled={false} onToggle={toggle} leftLabel={"on"} rightLabel={"off"}/>);
    var name = wrapper.find('[name="switchToggle"]');
    expect(name.last().props().disabled).toBeTruthy()
  });

  it("test if element triggers onToggle when changed (default)", () => {
    var toggle = jest.fn();
    var wrapper = shallow(<SwitchButton isOn={true} onToggle={toggle} leftLabel={"on"} rightLabel={"off"}/>);
    var name = wrapper.find('[name="switchToggle"]');
    name.last().simulate('change');
    expect(toggle).toBeCalled();
    expect(name.last().props().disabled).toBeFalsy();
    expect(name.last().props().checked).toBeTruthy();
  });

  it("test if element triggers onToggle when changed (negative)", () => {
    var toggle = jest.fn();
    var wrapper = shallow(<SwitchButton isOn={false} onToggle={toggle} leftLabel={"on"} rightLabel={"off"}/>);
    var name = wrapper.find('[name="switchToggle"]');
    name.first().simulate('change');
    expect(toggle).toBeCalled();
    expect(name.last().props().disabled).toBeFalsy();
    expect(name.first().props().checked).toBeTruthy();
  });

});
