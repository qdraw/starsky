import { render, screen } from "@testing-library/react";
import { ChangeEvent } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import { RawJpegMode } from "../../../interfaces/ICollectionsOpenType";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import * as SwitchButton from "../../atoms/switch-button/switch-button";
import PreferencesAppSettingsDesktop, {
  ToggleCollections,
  UpdateDefaultEditorPhotos
} from "./preference-app-settings-desktop";

describe("PreferencesAppSettingsDesktop", () => {
  it("should render correctly with provided props", () => {
    const switchButtonSpy = jest.spyOn(SwitchButton, "default").mockImplementationOnce(() => {
      return <></>;
    });

    render(<PreferencesAppSettingsDesktop />);

    expect(switchButtonSpy).toHaveBeenCalled();
  });

  it("should render MessageSwitchButtonDesktopApplicationDescription when appSettings.useLocalDesktop is true", () => {
    const mockGetIConnectionDefaultAppSettings = {
      statusCode: 200,
      data: {
        useLocalDesktop: true,
        defaultDesktopEditor: []
      }
    } as IConnectionDefault;

    const mockGetIConnectionDefaultPermissions = {
      statusCode: 200,
      data: null
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefaultAppSettings)
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions);

    const component = render(<PreferencesAppSettingsDesktop />);

    expect(
      screen.findByTestId("preference-app-settings-desktop-use-local-desktop-true")
    ).toBeTruthy();

    expect(useFetchSpy).toHaveBeenCalled();
    expect(useFetchSpy).toHaveBeenCalledTimes(2);

    component.unmount();
  });

  it("should render MessageSwitchButtonDesktopApplicationDescription when appSettings.useLocalDesktop is false", () => {
    const mockGetIConnectionDefaultAppSettings = {
      statusCode: 200,
      data: {
        useLocalDesktop: false,
        defaultDesktopEditor: []
      }
    } as IConnectionDefault;

    const mockGetIConnectionDefaultPermissions = {
      statusCode: 200,
      data: null
    } as IConnectionDefault;
    const useFetchSpy = jest
      .spyOn(useFetch, "default")
      .mockReset()
      .mockImplementationOnce(() => mockGetIConnectionDefaultAppSettings)
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions);

    const component = render(<PreferencesAppSettingsDesktop />);

    expect(
      screen.findByTestId("preference-app-settings-desktop-use-local-desktop-false")
    ).toBeTruthy();

    expect(useFetchSpy).toHaveBeenCalled();
    expect(useFetchSpy).toHaveBeenCalledTimes(2);

    component.unmount();
  });
});

describe("FetchPost Function", () => {
  it("should call FetchPost with correct URL and bodyParams for updateDefaultEditorPhotos function", async () => {
    const value = {
      target: { innerText: "NewApplicationPath" }
    } as unknown as ChangeEvent<HTMLDivElement>;
    const defaultDesktopEditor = [
      {
        applicationPath: "SampleApplicationPath",
        imageFormats: [ImageFormat.jpg, ImageFormat.png]
      }
    ];
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await UpdateDefaultEditorPhotos(value, jest.fn(), "", "", defaultDesktopEditor);

    expect(spyFetchPost).toHaveBeenCalledWith(
      expect.stringContaining("api/env"),
      expect.any(String)
    );
  });

  it("should call FetchPost with correct URL and bodyParams for toggleCollections function", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);
    const appSettings = {
      desktopCollectionsOpen: RawJpegMode.Default,
      useLocalDesktop: true
    } as unknown as IAppSettings;

    await ToggleCollections(true, jest.fn(), "", "", appSettings);

    expect(spyFetchPost).toHaveBeenCalledWith(
      expect.stringContaining("api/env"),
      expect.any(String)
    );
  });
});
