import { createEvent, fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import * as useFolderPicker from "../../../hooks/use-folder-picker";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import * as FetchPostModule from "../../../shared/fetch/fetch-post";
import * as UrlQueryModule from "../../../shared/url/url-query";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesAppSettingsStorageFolder, {
  ChangeSetting
} from "./preferences-app-settings-storage-folder";

describe("PreferencesAppSettings", () => {
  beforeEach(() => {
    jest.restoreAllMocks();
    jest.spyOn(useFolderPicker, "useFolderPicker").mockReturnValue({
      isNativeApp: () => false,
      requestFolderSelection: jest.fn()
    });
  });

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

      const storageFolder = screen.getByTestId("storage-folder-picker") as HTMLElement;
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

      const storageFolder = screen.getByTestId("storage-folder-picker") as HTMLElement;

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

      const storageFolder = screen.getByTestId("storage-folder-picker") as HTMLElement;

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
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockResolvedValue({
        statusCode: 200,
        data: null
      } as IConnectionDefault);
      const statusCode = await ChangeSetting(value, name);
      expect(statusCode).toBe(200);
      expect(fetchPostSpy).toHaveBeenCalled();
    });
  });

  describe("ChangeSetting", () => {
    let fetchPostSpy: jest.SpyInstance;
    let urlQuerySpy: jest.SpyInstance;

    beforeAll(() => {
      urlQuerySpy = jest
        .spyOn(UrlQueryModule, "UrlQuery")
        .mockReset()
        .mockImplementation(
          () => ({ UrlApiAppSettings: () => "/starsky/api/env/" }) as unknown as UrlQuery
        );
    });

    afterAll(() => {
      urlQuerySpy.mockRestore();
    });

    beforeEach(() => {
      fetchPostSpy = jest.spyOn(FetchPostModule, "default");
      fetchPostSpy.mockClear();
    });

    afterEach(() => {
      fetchPostSpy.mockRestore();
    });

    it("sends a single string value with provided name and returns status code", async () => {
      fetchPostSpy.mockResolvedValue({ statusCode: 200 });

      const status = await ChangeSetting("val", "name");

      expect(status).toBe(200);
      expect(fetchPostSpy).toHaveBeenCalledWith("/starsky/api/env/", "name=val");
    });

    it("sends a single string value with empty name when name omitted", async () => {
      fetchPostSpy.mockResolvedValue({ statusCode: 202 });

      const status = await ChangeSetting("val");

      expect(status).toBe(202);
      expect(fetchPostSpy).toHaveBeenCalledWith("/starsky/api/env/", "=val");
    });

    it("sends object values and skips null entries", async () => {
      fetchPostSpy.mockResolvedValue({ statusCode: 201 });

      const payload = { a: "1", b: null, c: "3" } as Record<string, string | null>;
      const status = await ChangeSetting(payload);

      expect(status).toBe(201);
      expect(fetchPostSpy).toHaveBeenCalledWith("/starsky/api/env/", "a=1&c=3");
    });

    it("returns undefined when FetchPost returns undefined", async () => {
      fetchPostSpy.mockResolvedValue(undefined);

      const status = await ChangeSetting({ foo: "bar" });

      expect(status).toBeUndefined();
      expect(fetchPostSpy).toHaveBeenCalledWith("/starsky/api/env/", "foo=bar");
    });
  });
});
