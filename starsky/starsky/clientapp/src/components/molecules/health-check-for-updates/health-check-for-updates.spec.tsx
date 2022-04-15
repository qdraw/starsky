import { render } from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as Notification from "../../atoms/notification/notification";
import HealthCheckForUpdates, {
  CheckForUpdatesLocalStorageName
} from "./health-check-for-updates";

describe("HealthCheckForUpdates", () => {
  it("renders (without state component)", () => {
    render(<HealthCheckForUpdates />);
  });

  describe("with Context", () => {
    it("Default not shown", () => {
      const mockGetIConnectionDefault = {
        statusCode: 200,
        data: null
      } as IConnectionDefault;

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <></>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = render(<HealthCheckForUpdates />);

      expect(notificationSpy).toBeCalledTimes(0);

      expect(useFetchSpy).toBeCalled();
      component.unmount();
    });

    it("Default shown when getting status 202", () => {
      const mockGetIConnectionDefault = {
        statusCode: 202,
        data: null
      } as IConnectionDefault;

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <></>);

      var component = render(<HealthCheckForUpdates></HealthCheckForUpdates>);

      expect(notificationSpy).toBeCalledTimes(1);

      expect(useFetchSpy).toBeCalled();
      component.unmount();
    });

    it("Click on close and expect that date is set in localstorage", () => {
      const mockGetIConnectionDefault = {
        statusCode: 202,
        data: null
      } as IConnectionDefault;

      jest.spyOn(Notification, "default").mockImplementationOnce((arg) => {
        if (!arg || !arg.callback) return null;
        arg.callback();
        return <></>;
      });
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = render(<HealthCheckForUpdates />);
      component.unmount();

      var item = localStorage.getItem(CheckForUpdatesLocalStorageName);
      if (!item) throw new Error("item should not be null");
      expect(parseInt(item) > 1604424674178).toBeTruthy(); // 3 nov '20

      localStorage.removeItem(CheckForUpdatesLocalStorageName);
    });

    it("Compontent should not shown when date is set in localstorage", () => {
      localStorage.setItem(
        CheckForUpdatesLocalStorageName,
        Date.now().toString()
      );
      const mockGetIConnectionDefault = {
        statusCode: 202,
        data: null
      } as IConnectionDefault;
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <></>);

      var component = render(<HealthCheckForUpdates />);

      expect(notificationSpy).toBeCalledTimes(0);

      component.unmount();
      localStorage.removeItem(CheckForUpdatesLocalStorageName);
    });

    it("Default shown when getting status 202 and should ignore non valid sessionStorageItem", () => {
      // This is an non valid Session storage item
      localStorage.setItem(CheckForUpdatesLocalStorageName, "non valid number");

      const mockGetIConnectionDefault = {
        statusCode: 202,
        data: null
      } as IConnectionDefault;

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <>t</>);

      var component = render(<HealthCheckForUpdates></HealthCheckForUpdates>);

      expect(notificationSpy).toBeCalledTimes(1);

      expect(useFetchSpy).toBeCalled();
      component.unmount();
      localStorage.removeItem(CheckForUpdatesLocalStorageName);
    });

    it("There a no links in the Notification when using electron", () => {
      // This is the difference
      (window as any).isElectron = true;

      const mockGetIConnectionDefault = {
        statusCode: 202,
        data: null
      } as IConnectionDefault;

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => <>t</>);

      const useFetchSpy = jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);
      var component = render(<HealthCheckForUpdates></HealthCheckForUpdates>);

      expect(notificationSpy).toBeCalledTimes(1);

      expect(useFetchSpy).toBeCalled();

      component.unmount();
      (window as any).isElectron = null;
    });
  });
});
