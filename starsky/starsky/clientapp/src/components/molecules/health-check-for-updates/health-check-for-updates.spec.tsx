import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../../../hooks/use-fetch';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as Notification from '../../atoms/notification/notification';
import HealthCheckForUpdates, { CheckForUpdatesLocalStorageName } from './health-check-for-updates';

describe("HealthCheckForUpdates", () => {

  it("renders (without state component)", () => {
    shallow(<HealthCheckForUpdates />)
  });

  describe("with Context", () => {
    it("Default not shown ", () => {

      const mockGetIConnectionDefault = {
        statusCode: 200, data: null
      } as IConnectionDefault;

      const useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)

      expect(component.exists(Notification.default)).toBeFalsy();

      expect(useFetchSpy).toBeCalled();
      component.unmount()
    });

    it("Default shown when getting status 202 ", () => {

      const mockGetIConnectionDefault = {
        statusCode: 202, data: null
      } as IConnectionDefault;

      const useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)

      expect(component.exists(Notification.default)).toBeTruthy();

      // there is a link to github
      expect(component.find("a")).toBeTruthy()

      expect(useFetchSpy).toBeCalled();
      component.unmount()
    });

    it("Click on close and expect that date is set in localstorage", () => {
      const mockGetIConnectionDefault = {
        statusCode: 202, data: null
      } as IConnectionDefault;

      jest.spyOn(Notification, 'default').mockImplementationOnce((arg) => {
        if (!arg || !arg.callback) return null;
        arg.callback()
        return <></>
      })
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)
      component.unmount();

      var item = localStorage.getItem(CheckForUpdatesLocalStorageName);
      if (!item) throw new Error("item should not be null")
      expect(parseInt(item) > 1604424674178) // 3 nov '20

      localStorage.removeItem(CheckForUpdatesLocalStorageName)
    });

    it("Compontent should not shown when date is set in localstorage", () => {
      localStorage.setItem(CheckForUpdatesLocalStorageName, Date.now().toString())
      const mockGetIConnectionDefault = {
        statusCode: 202, data: null
      } as IConnectionDefault;
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)
      expect(component.exists(Notification.default)).toBeFalsy();

      component.unmount();
      localStorage.removeItem(CheckForUpdatesLocalStorageName)
    });

    it("Default shown when getting status 202 and should ignore non valid sessionStorageItem", () => {

      // This is an non valid Session storage item
      localStorage.setItem(CheckForUpdatesLocalStorageName, "non valid number")

      const mockGetIConnectionDefault = {
        statusCode: 202, data: null
      } as IConnectionDefault;

      const useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)

      expect(component.exists(Notification.default)).toBeTruthy();

      expect(useFetchSpy).toBeCalled();
      component.unmount();
      localStorage.removeItem(CheckForUpdatesLocalStorageName)

    });

    it("Default shown when getting status 202 and should ignore non valid sessionStorageItem", () => {

      (window as any).isElectron = true;

      const mockGetIConnectionDefault = {
        statusCode: 202, data: null
      } as IConnectionDefault;

      const useFetchSpy = jest.spyOn(useFetch, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = mount(<HealthCheckForUpdates />)

      expect(component.exists(Notification.default)).toBeTruthy();

      // there are no links here
      var aHrefs = component.find("a").length;
      expect(aHrefs).toBeFalsy()

      expect(useFetchSpy).toBeCalled();

      component.unmount();
      (window as any).isElectron = null;

    });
  });
});