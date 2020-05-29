import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
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

    it("Your password has been successfully changed", async () => {
      var component = mount(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: { success: true } });
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);


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

      // need to await
      await act(async () => {
        await component.find('form [type="submit"]').first().simulate('submit');
      });

      // force update to get the right text
      component.update();

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1");

      expect(component.find('.warning-box').text()).toBe("Your password has been successfully changed")

      component.unmount();
    });

    it("Enter your current password", async () => {
      var component = mount(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 401, data: null });
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);


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

      // need to await
      await act(async () => {
        await component.find('form [type="submit"]').first().simulate('submit');
      });

      // force update to get the right text
      component.update();

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1");

      expect(component.find('.warning-box').text()).toBe("Enter your current password")

      component.unmount();
    });

    it("Modal Error - The new password does not meet the criteria", async () => {
      var component = mount(<PreferencesPassword />);
      // spy on fetch
      // use this using => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 400, data: null });
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);


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

      // need to await
      await act(async () => {
        await component.find('form [type="submit"]').first().simulate('submit');
      });

      // force update to get the right text
      component.update();

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlAccountChangeSecret(),
        "Password=12345&ChangedPassword=password1&ChangedConfirmPassword=password1");

      expect(component.find('.warning-box').text()).toBe("The new password does not meet the criteria")

      component.unmount();
    });

  });
});