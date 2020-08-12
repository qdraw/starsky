import { shallow } from 'enzyme';
import React from 'react';
import Select from './select';

describe("SwitchButton", () => {

  it("renders", () => {
    shallow(<Select selectOptions={[]} />)
  });

  it("trigger change", () => {
    var outputSpy = jest.fn();
    var component = shallow(<Select selectOptions={["Test"]} callback={outputSpy} />);
    component.find('select').simulate('change', { target: { value: 'test' } });

    expect(outputSpy).toBeCalled();
    expect(outputSpy).toBeCalledWith('test');
  });

  it("trigger change (no callback)", () => {
    var outputSpy = jest.fn();
    var component = shallow(<Select selectOptions={[]} />);
    component.find('select').simulate('change', { target: { value: 'test' } });

    expect(outputSpy).toBeCalledTimes(0);
  });

  it("find option", () => {
    var component = shallow(<Select selectOptions={["Test"]} />);

    expect(component.find('option').text()).toBe('Test')
  });
});