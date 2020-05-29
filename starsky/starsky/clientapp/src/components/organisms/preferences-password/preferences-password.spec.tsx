import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import * as FetchPost from '../../../shared/fetch-post';
import PreferencesPassword from './preferences-password';

describe("PreferencesPassword", () => {
  it("renders", () => {
    shallow(<PreferencesPassword />)
  });

  describe("context", () => {

    it("default nothing entered", () => {
      var component = mount(<PreferencesPassword />);

      component.find('form [type="submit"]').first().simulate('submit');

      expect(component.find('.warning-box').text()).toBe("Enter the current and new password");
      component.unmount();

    });

    it("The passwords do not match", () => {
      var component = mount(<PreferencesPassword />);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="password"]').getDOMNode() as HTMLInputElement).value = "12345";
        component.find('input[name="password"]').first().simulate('change');
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="changed-password"]').getDOMNode() as HTMLInputElement).value = "password1";
        component.find('input[name="changed-password"]').first().simulate('change');
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="changed-confirm-password"]').getDOMNode() as HTMLInputElement).value = "something-else";
        component.find('input[name="changed-confirm-password"]').first().simulate('change');
      });

      component.find('form [type="submit"]').first().simulate('submit');

      expect(component.find('.warning-box').text()).toBe("The passwords do not match")

      component.unmount();
    });

    xit("The passwords do not ma111tch", () => {
      var component = mount(<PreferencesPassword />);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="password"]').getDOMNode() as HTMLInputElement).value = "12345";
        component.find('input[name="password"]').first().simulate('change');
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="changed-password"]').getDOMNode() as HTMLInputElement).value = "password1";
        component.find('input[name="changed-password"]').first().simulate('change');
      });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        (component.find('input[name="changed-confirm-password"]').getDOMNode() as HTMLInputElement).value = "password1";
        component.find('input[name="changed-confirm-password"]').first().simulate('change');
      });

      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: [{ status: IExifStatus.Ok }] as IFileIndexItem[] });
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        component.find('form [type="submit"]').first().simulate('submit');
      });

      expect(fetchPostSpy).toBeCalled();

      console.log(component.html())
      // component.update();

      expect(component.find('.warning-box').text()).toBe("The passwords do not match")

    });

  });
});