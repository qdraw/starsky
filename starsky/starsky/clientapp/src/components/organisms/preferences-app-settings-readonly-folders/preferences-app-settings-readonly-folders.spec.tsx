import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesAppSettingsReadonlyFolders, {
  ChangeSettingReadOnlyFolders
} from "./preferences-app-settings-readonly-folders";

describe("PreferencesAppSettingsReadonlyFolders", () => {
  it("renders without crashing", () => {
    const component = render(<PreferencesAppSettingsReadonlyFolders />);
    expect(component).toBeTruthy();
    act(() => {
      component.unmount();
    });
  });

  describe("context", () => {
    it("renders existing folders as rows", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024", "/2023"] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.queryAllByTestId("readonly-folder-row")).toHaveLength(2);

      act(() => {
        component.unmount();
      });
    });

    it("shows none-message when no folders and user has no permission", () => {
      const permissions = { statusCode: 200, data: [] } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: [] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.getByTestId("readonly-folders-none")).toBeTruthy();

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
        data: { readOnlyFolders: [] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.getByTestId("readonly-folder-add")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("does not show add button without permission", () => {
      const permissions = { statusCode: 200, data: [] } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: [] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.queryByTestId("readonly-folder-add")).toBeNull();

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
        data: { readOnlyFolders: [] }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      act(() => {
        fireEvent.click(screen.getByTestId("readonly-folder-add"));
      });

      expect(screen.queryAllByTestId("readonly-folder-row")).toHaveLength(1);

      act(() => {
        component.unmount();
      });
    });

    it("does not show remove buttons without permission", () => {
      const permissions = { statusCode: 200, data: [] } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.queryByTestId("readonly-folder-remove-0")).toBeNull();

      act(() => {
        component.unmount();
      });
    });

    it("shows remove buttons when user has permission", () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings)
        .mockImplementationOnce(() => permissions)
        .mockImplementationOnce(() => appSettings);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      expect(screen.getByTestId("readonly-folder-remove-0")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });

    it("removes a row when remove button is clicked and posts updated folders", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
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

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      await act(async () => {
        fireEvent.click(screen.getByTestId("readonly-folder-remove-0"));
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(screen.queryAllByTestId("readonly-folder-row")).toHaveLength(0);

      act(() => {
        component.unmount();
      });
    });

    it("saves on blur and shows re-sync warning", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as IConnectionDefault;

      jest.spyOn(useFetch, "default").mockImplementation((url) => {
        if (url === new UrlQuery().UrlAccountPermissions()) return permissions;
        return appSettings;
      });

      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      const formControl = screen.getByTestId("form-control");
      formControl.innerText = "/renamed";
      await act(async () => {
        fireEvent.focusOut(formControl);
        await mockResult;
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(screen.getByTestId("readonly-folders-changed")).toBeTruthy();

      act(() => {
        component.unmount();
      });
    });
  });

  describe("error handling", () => {
    it("shows error box when server returns non-200", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as unknown as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementation((url: string) =>
          url.includes("permissions") ? permissions : appSettings
        );

      jest
        .spyOn(FetchPost, "default")
        .mockImplementation(() =>
          Promise.resolve({ statusCode: 403, data: null } as IConnectionDefault)
        );

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      const formControl = await screen.findByTestId("form-control");
      fireEvent.blur(formControl, { target: { innerText: "/etc/shadow" } });

      await screen.findByTestId("readonly-folders-error");
      expect(screen.queryByTestId("readonly-folders-changed")).toBeNull();

      act(() => {
        component.unmount();
      });
    });

    it("shows error box when saving fails with a rejected request", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as unknown as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementation((url: string) =>
          url.includes("permissions") ? permissions : appSettings
        );

      jest.spyOn(FetchPost, "default").mockRejectedValue(new Error("network failure"));

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      const formControl = await screen.findByTestId("form-control");
      fireEvent.blur(formControl, { target: { innerText: "/renamed" } });

      await screen.findByTestId("readonly-folders-error");
      expect(screen.queryByTestId("readonly-folders-changed")).toBeNull();

      act(() => {
        component.unmount();
      });
    });

    it("does not show error box when server returns 200", async () => {
      const permissions = {
        statusCode: 200,
        data: ["AppSettingsWrite"]
      } as IConnectionDefault;
      const appSettings = {
        statusCode: 200,
        data: { readOnlyFolders: ["/2024"] }
      } as unknown as IConnectionDefault;

      jest
        .spyOn(useFetch, "default")
        .mockImplementation((url: string) =>
          url.includes("permissions") ? permissions : appSettings
        );

      jest
        .spyOn(FetchPost, "default")
        .mockImplementation(() =>
          Promise.resolve({ statusCode: 200, data: null } as IConnectionDefault)
        );

      const component = render(<PreferencesAppSettingsReadonlyFolders />);

      const formControl = await screen.findByTestId("form-control");
      fireEvent.blur(formControl, { target: { innerText: "/2024" } });

      await screen.findByTestId("readonly-folders-changed");
      expect(screen.queryByTestId("readonly-folders-error")).toBeNull();

      act(() => {
        component.unmount();
      });
    });
  });

  describe("ChangeSettingReadOnlyFolders", () => {
    it("should post folders as indexed form-urlencoded", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      const statusCode = await ChangeSettingReadOnlyFolders([{ id: 0, folder: "/2024" }]);

      expect(statusCode).toBe(200);
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "ReadOnlyFolders%5B0%5D=%2F2024"
      );
    });

    it("should skip rows with empty folder", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      await ChangeSettingReadOnlyFolders([
        { id: 0, folder: "" },
        { id: 1, folder: "/2023" }
      ]);

      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "ReadOnlyFolders%5B0%5D=%2F2023"
      );
    });

    it("should post empty body when all rows are empty", async () => {
      const mockResult: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200,
        data: null
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockResult);

      await ChangeSettingReadOnlyFolders([{ id: 0, folder: "" }]);

      expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().UrlApiAppSettings(), "");
    });
  });
});
