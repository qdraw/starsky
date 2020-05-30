import { mount, shallow } from "enzyme";
import React from 'react';
import * as useFetch from '../../../hooks/use-fetch';
import { IConnectionDefault, newIConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
import PreferencesAppSettings from './preferences-app-settings';

describe("PreferencesAppSettings", () => {

  it("renders", () => {
    shallow(<PreferencesAppSettings />)
  });

  describe("context", () => {

    it("disabled by default", () => {
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      var component = mount(<PreferencesAppSettings />);

      expect((component.find('input[name="verbose"]').first().getDOMNode() as HTMLInputElement).disabled).toBeTruthy();

      component.unmount()
    });

    it("not disabled when admin", () => {
      var connectionDefault = { statusCode: 200, data: ["AppSettingsWrite"] } as IConnectionDefault;
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault)

      var component = mount(<PreferencesAppSettings />);

      expect((component.find('input[name="verbose"]').first().getDOMNode() as HTMLInputElement).disabled).toBeFalsy();

      component.unmount()
    });

    it("filled right data", () => {
      var permissions = { statusCode: 200, data: ["AppSettingsWrite"] } as IConnectionDefault;
      var appSettings = {
        statusCode: 200, data: {
          verbose: true,
          storageFolder: 'test'
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      var component = mount(<PreferencesAppSettings />);

      expect(component.find('[data-name="storageFolder"]').text()).toBe('test');

      component.unmount()
    });

    it("change storageFolder", () => {
      var permissions = { statusCode: 200, data: ["AppSettingsWrite"] } as IConnectionDefault;
      var appSettings = {
        statusCode: 200, data: {
          verbose: true,
          storageFolder: 'test'
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var component = mount(<PreferencesAppSettings />);

      var storageFolderForm = component.find('[data-name="storageFolder"]');

      (storageFolderForm.getDOMNode() as HTMLInputElement).innerText = "12345";
      storageFolderForm.first().simulate('blur');

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlApiAppSettings(), "storageFolder=12345");


    });

  });
});