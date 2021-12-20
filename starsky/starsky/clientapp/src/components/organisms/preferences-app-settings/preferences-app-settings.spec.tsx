import {
  act,
  createEvent,
  fireEvent,
  render,
  waitFor
} from "@testing-library/react";
import React from "react";
import * as useFetch from "../../../hooks/use-fetch";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import PreferencesAppSettings from "./preferences-app-settings";

describe("PreferencesAppSettings", () => {
  it("renders", () => {
    render(<PreferencesAppSettings />);
  });

  describe("context", () => {
    it("disabled by default", () => {
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      var component = render(<PreferencesAppSettings />);

      const switchButtons = component.queryAllByTestId("switch-button-right");

      const verbose = switchButtons.find(
        (p) => p.getAttribute("name") === "verbose"
      ) as HTMLInputElement;

      expect(verbose.disabled).toBeTruthy();

      component.unmount();
    });

    it("not disabled when admin", () => {
      var connectionDefault = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault)
        .mockImplementationOnce(() => connectionDefault);

      var component = render(<PreferencesAppSettings />);

      const switchButtons = component.queryAllByTestId("switch-button-right");

      const verbose = switchButtons.find(
        (p) => p.getAttribute("name") === "verbose"
      ) as HTMLInputElement;

      expect(verbose.disabled).toBeFalsy();

      component.unmount();
    });

    it("filled right data", () => {
      var permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      var appSettings = {
        statusCode: 200,
        data: {
          verbose: true,
          storageFolder: "test"
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      var component = render(<PreferencesAppSettings />);

      const formControls = component.queryAllByTestId("form-control");

      const storageFolder = formControls.find(
        (p) => p.getAttribute("data-name") === "storageFolder"
      ) as HTMLElement;
      expect(storageFolder).not.toBeNull();

      expect(storageFolder.textContent).toBe("test");

      act(() => {
        component.unmount();
      });
    });

    it("change storageFolder", async () => {
      var permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      var appSettings = {
        statusCode: 200,
        data: {
          verbose: true,
          storageFolder: "test",
          storageFolderAllowEdit: true
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve(newIConnectionDefault());
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var component = render(<PreferencesAppSettings />);

      const formControls = component
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "storageFolder");
      const storageFolder = formControls as HTMLInputElement[][0];

      storageFolder.innerText = "12345";
      const blurEventYear = createEvent.focusOut(storageFolder, {
        textContent: "12345"
      });

      await act(async () => {
        await fireEvent(storageFolder, blurEventYear);
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "storageFolder=12345"
      );
      act(() => {
        component.unmount();
      });
    });

    it("change storageFolder failed", async () => {
      var permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      var appSettings = {
        statusCode: 200,
        data: {
          verbose: true,
          storageFolder: "test",
          storageFolderAllowEdit: true
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      // This fails -->
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ ...newIConnectionDefault(), statusCode: 404 });
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var component = render(<PreferencesAppSettings />);

      const formControls = component
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "storageFolder");
      const storageFolder = formControls as HTMLInputElement[][0];

      storageFolder.innerText = "12345";
      const blurEventYear = createEvent.focusOut(storageFolder, {
        textContent: "12345"
      });

      await act(async () => {
        await fireEvent(storageFolder, blurEventYear);
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "storageFolder=12345"
      );

      // if failed show extra storage id
      expect(component.queryByTestId("storage-not-found")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("toggle verbose", async () => {
      var permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      var appSettings = {
        statusCode: 200,
        data: {
          verbose: true,
          storageFolder: "test"
        }
      } as IConnectionDefault;

      // usage ==> import * as useFetch from '../../../hooks/use-fetch';
      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      var component = render(<PreferencesAppSettings />);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            statusCode: 400,
            data: null
          });
        });

      const switchButtons = component.queryAllByTestId("switch-button-right");

      const verbose = switchButtons.find(
        (p) => p.getAttribute("name") === "verbose"
      ) as HTMLElement;

      verbose.click();

      await waitFor(() => expect(fetchPostSpy).toBeCalled());

      component.unmount();
    });
  });
});
