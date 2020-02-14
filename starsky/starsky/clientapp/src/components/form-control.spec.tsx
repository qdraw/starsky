import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import FormControl from './form-control';

describe("FormControl", () => {

  it("renders", () => {
    shallow(<FormControl contentEditable={true} onBlur={() => { }} name="test">&nbsp;</FormControl>)
  });

  describe("with events", () => {
    it("limitLengthKey - keydown max limit/preventDefault", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">123456789</FormControl>);

      var preventDefaultSpy = jest.fn();

      act(() => {
        component.getDOMNode().innerHTML = "12345678901";
        component.simulate('keydown', { key: 'x', preventDefault: preventDefaultSpy })
      });

      expect(preventDefaultSpy).toBeCalled();

      component.unmount();
    });

    it("limitLengthKey - keydown ok", () => {

      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={() => { }} name="test">123456</FormControl>);

      var preventDefaultSpy = jest.fn();

      act(() => {
        component.getDOMNode().innerHTML = "1234567";
        component.simulate('keydown', { key: 'x', preventDefault: preventDefaultSpy })
      });

      expect(preventDefaultSpy).toBeCalledTimes(0);

      component.unmount();
    });

    it("limitLengthPaste - copy -> paste limit/preventDefault", () => {

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

    it("limitLengthPaste - copy -> paste ok", () => {

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

    it("limitLengthBlur - onBlur pushed/ok", () => {
      var onBlurSpy = jest.fn();
      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test">abcdefghi</FormControl>);

      act(() => {
        component.simulate('blur')
      });

      expect(component.exists('.warning-box')).toBeFalsy();
      expect(onBlurSpy).toBeCalled();

      onBlurSpy.mockReset();
      component.unmount();
    });

    it("limitLengthBlur - onBlur limit/preventDefault", () => {
      var onBlurSpy = jest.fn();
      var component = mount(<FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test123">012345678919000000</FormControl>);

      // need to dispatch on child element
      component.find(".form-control").simulate('blur');

      expect(onBlurSpy).toBeCalledTimes(0);
      expect(component.exists('.warning-box')).toBeTruthy();

      component.unmount();
    });

    it("limitLengthBlur - onBlur limit", () => {

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