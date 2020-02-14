import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import FormControl from './form-control';

describe("FormControl", () => {

  it("renders", () => {
    shallow(<FormControl contentEditable={true} onBlur={() => { }} name="test">&nbsp;</FormControl>)
  });

  describe("with events", () => {
    it("keydown max limit/preventDefault", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">123456789</FormControl>);

      var preventDefaultSpy = jest.fn();

      act(() => {
        component.getDOMNode().innerHTML = "12345678901";
        component.simulate('keydown', { key: 'x', preventDefault: preventDefaultSpy })
      });

      expect(preventDefaultSpy).toBeCalled();

      component.unmount();
    });

    it("copy -> paste limit/preventDefault", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">123456789</FormControl>);

      var preventDefaultSpy = jest.fn();

      var mockDataTransfer = {
        getData: () => {
          return "t"
        }
      };

      act(() => {
        component.simulate('paste', { clipboardData: mockDataTransfer, preventDefault: preventDefaultSpy })
      });

      expect(preventDefaultSpy).toBeCalled()

      component.unmount();
    });

    it("copy -> paste ok", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">12345</FormControl>);

      var preventDefaultSpy = jest.fn();

      var mockDataTransfer = {
        getData: () => {
          return "t"
        }
      };

      act(() => {
        component.simulate('paste', { clipboardData: mockDataTransfer, preventDefault: preventDefaultSpy })
      });

      expect(preventDefaultSpy).toBeCalledTimes(0)

      component.unmount();
    });

    it("onBlur limit/preventDefault", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">1234567890___</FormControl>);

      act(() => {
        component.simulate('blur')
      });

      expect(component.exists('.warning-box')).toBeTruthy();

      component.unmount();
    });

    it("onBlur pushed/ok", () => {

      var onBlurSpy = jest.fn();
      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test">123456789</FormControl>);

      act(() => {
        component.simulate('blur')
      });

      expect(component.exists('.warning-box')).toBeFalsy();
      expect(onBlurSpy).toBeCalled();

      component.unmount();
    });

    it("onBlur limit", () => {

      var onBlurSpy = jest.fn();
      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test">1234567890123</FormControl>);

      act(() => {
        component.simulate('blur')
      });

      expect(component.exists('.warning-box')).toBeTruthy();
      expect(onBlurSpy).toBeCalledTimes(0)

      component.unmount();
    });

  });

});