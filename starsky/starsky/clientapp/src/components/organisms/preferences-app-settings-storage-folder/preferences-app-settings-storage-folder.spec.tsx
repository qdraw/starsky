import { act, createEvent, fireEvent, render, screen } from "@testing-library/react";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import PreferencesAppSettingsStorageFolder, {
  ChangeSetting
} from "./preferences-app-settings-storage-folder";

describe("PreferencesAppSettings", () => {
  it("renders", () => {
    render(<PreferencesAppSettingsStorageFolder />);
  });

  describe("context", () => {
    it("filled right data", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
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

      const component = render(<PreferencesAppSettingsStorageFolder />);

      const formControls = screen.queryAllByTestId("form-control");

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
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
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
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<PreferencesAppSettingsStorageFolder />);

      const formControls = screen
        .queryAllByTestId("form-control")
        .find((p) => p.getAttribute("data-name") === "storageFolder");
      const storageFolder = formControls as HTMLInputElement[][0];

      storageFolder.innerText = "12345";
      const blurEventYear = createEvent.focusOut(storageFolder, {
        textContent: "12345"
      });

      await fireEvent(storageFolder, blurEventYear);

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "storageFolder=12345"
      );
      act(() => {
        component.unmount();
      });
    });

    it("change storageFolder failed", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
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
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        statusCode: 404
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const component = render(<PreferencesAppSettingsStorageFolder />);

      const formControls = screen
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

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "storageFolder=12345"
      );

      // if failed show extra storage id
      expect(screen.getByTestId("storage-not-found")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });
  });

  describe("ChangeSetting", () => {
    it("should set value with provided name when name is provided", async () => {
      const value = "test value";
      const name = "test name";
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
        return Promise.resolve({
          statusCode: 200,
          data: null
        });
      });
      const statusCode = await ChangeSetting(value, name);
      expect(statusCode).toBe(200);
      expect(fetchPostSpy).toHaveBeenCalled();
    });
  });
});
