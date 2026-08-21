import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesAppSettingsStorageFolderMappings, {
  ChangeSettingMappings
} from "./preferences-app-settings-storage-folder-mappings";

describe("PreferencesAppSettingsStorageFolderMappings", () => {
  it("renders without crashing", () => {
    const component = render(<PreferencesAppSettingsStorageFolderMappings />);
    expect(component).toBeTruthy();
    act(() => {
      component.unmount();
    });
  });

  describe("context", () => {
    it("renders existing mappings as rows", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: {
            "/2024": "/data/archive/2024",
            "/2023": "/data/archive/2023"
          }
        }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const rows = screen.queryAllByTestId("storage-folder-mapping-row");
      expect(rows).toHaveLength(2);

      act(() => {
        component.unmount();
      });
    });

    it("shows add button when user has AppSettingsWrite permission", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { storageFolderMappings: {} }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      expect(screen.getByTestId("storage-folder-mapping-add")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("does not show add button without permission", () => {
      const permissions = {
        statusCode: 200,
        data: []
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { storageFolderMappings: {} }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      expect(screen.queryByTestId("storage-folder-mapping-add")).toBeNull();

      act(() => {
        component.unmount();
      });
    });

    it("adds a new row when add button is clicked", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { storageFolderMappings: {} }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const addButton = screen.getByTestId("storage-folder-mapping-add");

      act(() => {
        fireEvent.click(addButton);
      });

      const rows = screen.queryAllByTestId("storage-folder-mapping-row");
      expect(rows).toHaveLength(1);

      act(() => {
        component.unmount();
      });
    });

    it("does not show remove buttons without permission", () => {
      const permissions = {
        statusCode: 200,
        data: []
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: { "/2024": "/data/archive/2024" }
        }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      expect(screen.queryByTestId("storage-folder-mapping-remove-0")).toBeNull();

      act(() => {
        component.unmount();
      });
    });

    it("saves on subPath blur and shows re-sync warning", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: { "/2024": "/data/archive/2024" }
        }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const mockResult: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const formControls = screen.queryAllByTestId("form-control");
      const subPathControl = formControls.find(
        (el) => el.getAttribute("data-name") === "storageFolderMappings-subPath-0"
      ) as HTMLElement;

      subPathControl.innerText = "/renamed";
      await act(async () => {
        fireEvent.focusOut(subPathControl);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(screen.getByTestId("storage-mapping-changed")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("saves on physicalPath blur and shows re-sync warning", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: { "/2024": "/data/archive/2024" }
        }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const mockResult: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const formControls = screen.queryAllByTestId("form-control");
      const physicalPathControl = formControls.find(
        (el) => el.getAttribute("data-name") === "storageFolderMappings-physicalPath-0"
      ) as HTMLElement;

      physicalPathControl.innerText = "/data/new-location";
      await act(async () => {
        fireEvent.focusOut(physicalPathControl);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(screen.getByTestId("storage-mapping-changed")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("saves only edited row's subPath while keeping other rows unchanged", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: {
            "/2024": "/data/archive/2024",
            "/2023": "/data/archive/2023"
          }
        }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const mockResult: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const formControls = screen.queryAllByTestId("form-control");
      const subPathControl = formControls.find(
        (el) => el.getAttribute("data-name") === "storageFolderMappings-subPath-0"
      ) as HTMLElement;

      subPathControl.innerText = "/renamed";
      await act(async () => {
        fireEvent.focusOut(subPathControl);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      // Both rows should still exist
      expect(screen.queryAllByTestId("storage-folder-mapping-row")).toHaveLength(2);

      act(() => {
        component.unmount();
      });
    });

    it("saves only edited row's physicalPath while keeping other rows unchanged", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: {
            "/2024": "/data/archive/2024",
            "/2023": "/data/archive/2023"
          }
        }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const mockResult: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const formControls = screen.queryAllByTestId("form-control");
      const physicalPathControl = formControls.find(
        (el) => el.getAttribute("data-name") === "storageFolderMappings-physicalPath-0"
      ) as HTMLElement;

      physicalPathControl.innerText = "/data/new-location";
      await act(async () => {
        fireEvent.focusOut(physicalPathControl);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(screen.queryAllByTestId("storage-folder-mapping-row")).toHaveLength(2);

      act(() => {
        component.unmount();
      });
    });

    it("removes a row when remove button is clicked and posts updated mappings", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: {
          storageFolderMappings: {
            "/2024": "/data/archive/2024"
          }
        }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementation(() => permissions)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const mockResult: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsStorageFolderMappings />);

      const removeButton = screen.getByTestId("storage-folder-mapping-remove-0");

      await act(async () => {
        fireEvent.click(removeButton);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      const rows = screen.queryAllByTestId("storage-folder-mapping-row");
      expect(rows).toHaveLength(0);

      act(() => {
        component.unmount();
      });
    });
  });

  describe("ChangeSettingMappings", () => {
    it("should post all valid mappings as form-urlencoded", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const statusCode = await ChangeSettingMappings([
        { id: 0, subPath: "/2024", physicalPath: "/data/archive/2024" }
      ]);

      expect(statusCode).toBe(200);
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "StorageFolderMappings%5B%2F2024%5D=%2Fdata%2Farchive%2F2024"
      );
    });

    it("should skip rows with empty subPath or physicalPath", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      await ChangeSettingMappings([
        { id: 0, subPath: "", physicalPath: "/data/archive/2024" },
        { id: 1, subPath: "/2024", physicalPath: "" },
        { id: 2, subPath: "/2023", physicalPath: "/data/archive/2023" }
      ]);

      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "StorageFolderMappings%5B%2F2023%5D=%2Fdata%2Farchive%2F2023"
      );
    });

    it("should post empty body when all rows are invalid", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      await ChangeSettingMappings([{ id: 0, subPath: "", physicalPath: "" }]);

      expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().UrlApiAppSettings(), "");
    });
  });
});
