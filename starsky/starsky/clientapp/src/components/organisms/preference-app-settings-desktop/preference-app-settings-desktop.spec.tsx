import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ChangeEvent } from "react";
import * as useFetch from "../../../hooks/use-fetch";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import { RawJpegMode } from "../../../interfaces/ICollectionsOpenType";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as FormControl from "../../atoms/form-control/form-control";
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

  it("get application path from useFetch and display", () => {
    const mockGetIConnectionDefaultAppSettings = {
      statusCode: 200,
      data: {
        useLocalDesktop: true,
        defaultDesktopEditor: [
          {
            applicationPath: "/test",
            imageFormats: [ImageFormat.tiff]
          }
        ]
      } as unknown as IAppSettings
    } as IConnectionDefault;

    const formControlSpy = jest.spyOn(FormControl, "default").mockImplementationOnce(() => {
      return <></>;
    });

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

    expect(formControlSpy).toHaveBeenCalled();
    expect(formControlSpy).toHaveBeenCalledWith(
      {
        children: "/test",
        contentEditable: undefined,
        name: "tags",
        onBlur: expect.anything(),
        spellcheck: true
      },
      {}
    );

    expect(useFetchSpy).toHaveBeenCalled();
    expect(useFetchSpy).toHaveBeenCalledTimes(2);

    component.unmount();
  });

  it("give message when done with SwitchButton", async () => {
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
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions)
      .mockImplementationOnce(() => mockGetIConnectionDefaultAppSettings)
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions)
      .mockImplementationOnce(() => mockGetIConnectionDefaultAppSettings)
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions);

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);

    const switchButtonSpy = jest.spyOn(SwitchButton, "default").mockImplementationOnce((props) => {
      return <button data-test="switch-button-spy" onClick={() => props.onToggle(true)}></button>;
    });

    const component = render(<PreferencesAppSettingsDesktop />);

    expect(screen.queryByTestId("preference-app-settings-desktop-warning-box")).toBeFalsy();

    fireEvent.click(screen.getByTestId("switch-button-spy"));

    await waitFor(() => {
      expect(spyFetchPost).toHaveBeenCalled();
      expect(spyFetchPost).toHaveBeenCalledTimes(1);
      expect(spyFetchPost).toHaveBeenCalledWith(
        new UrlQuery().UrlApiAppSettings(),
        "desktopCollectionsOpen=2"
      );

      expect(screen.getByTestId("preference-app-settings-desktop-warning-box")).toBeTruthy();
    });

    expect(switchButtonSpy).toHaveBeenCalled();

    expect(useFetchSpy).toHaveBeenCalled();
    expect(useFetchSpy).toHaveBeenCalledTimes(6);

    component.unmount();
  });

  it("give message when done with FormControl", async () => {
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
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions)
      .mockImplementationOnce(() => mockGetIConnectionDefaultAppSettings)
      .mockImplementationOnce(() => mockGetIConnectionDefaultPermissions);

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);

    const switchButtonSpy = jest.spyOn(FormControl, "default").mockImplementationOnce((props) => {
      return (
        <button
          data-test="form-control-spy"
          onClick={() => {
            if (props.onBlur) {
              props?.onBlur({
                target: {
                  innerText: "test"
                }
              } as unknown as ChangeEvent<HTMLDivElement>);
            }
          }}
        ></button>
      );
    });

    const component = render(<PreferencesAppSettingsDesktop />);

    expect(screen.queryByTestId("preference-app-settings-desktop-warning-box")).toBeFalsy();

    fireEvent.click(screen.getByTestId("form-control-spy"));

    await waitFor(() => {
      const query =
        "DefaultDesktopEditor%5B0%5D.ImageFormats%5B0%5D=jpg&DefaultDesktopEditor%5B0%5D.ImageFormats%" +
        "5B1%5D=png&DefaultDesktopEditor%5B0%5D.ImageFormats%5B2%5D=bmp&DefaultDesktopEditor%5B0%5D.ImageFormats%5B3%5D=tiff&" +
        "DefaultDesktopEditor%5B0%5D.ApplicationPath=test";

      expect(spyFetchPost).toHaveBeenCalledTimes(1);
      expect(spyFetchPost).toHaveBeenCalledWith(new UrlQuery().UrlApiAppSettings(), query);

      expect(screen.getByTestId("preference-app-settings-desktop-warning-box")).toBeTruthy();
    });

    expect(switchButtonSpy).toHaveBeenCalled();

    expect(useFetchSpy).toHaveBeenCalled();
    expect(useFetchSpy).toHaveBeenCalledTimes(4);

    component.unmount();
  });
});

describe("updateDefaultEditorPhotos", () => {
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
      expect.stringContaining(new UrlQuery().UrlApiAppSettings()),
      expect.any(String)
    );
  });

  it("UpdateDefaultEditorPhotos call error if default defaultDesktopEditor is missing", async () => {
    const value = {
      target: { innerText: "NewApplicationPath" }
    } as unknown as ChangeEvent<HTMLDivElement>;
    const setIsMessage = jest.fn();
    await UpdateDefaultEditorPhotos(value, setIsMessage, "error_here", "");
    expect(setIsMessage).toHaveBeenCalled();
    expect(setIsMessage).toHaveBeenCalledWith("error_here");
  });

  it("UpdateDefaultEditorPhotos call error if fetchPost is Error 500", async () => {
    const value = {
      target: { innerText: "NewApplicationPath" }
    } as unknown as ChangeEvent<HTMLDivElement>;

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 500
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const setIsMessage = jest.fn();
    await UpdateDefaultEditorPhotos(value, setIsMessage, "error_here", "", []);
    expect(setIsMessage).toHaveBeenCalled();
    expect(setIsMessage).toHaveBeenCalledWith("error_here");

    expect(spyFetchPost).toHaveBeenCalled();
  });

  it("Create new item in Array if emthy array", async () => {
    const value = {
      target: { innerText: "test" }
    } as unknown as ChangeEvent<HTMLDivElement>;

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);

    const setIsMessage = jest.fn();
    await UpdateDefaultEditorPhotos(value, setIsMessage, "error_here", "success", []);
    expect(setIsMessage).toHaveBeenCalled();
    expect(setIsMessage).toHaveBeenCalledWith("success");

    expect(spyFetchPost).toHaveBeenCalled();
    const query =
      "DefaultDesktopEditor%5B0%5D.ImageFormats%5B0%5D=jpg&DefaultDesktopEditor%5B0%5D.ImageFormats%" +
      "5B1%5D=png&DefaultDesktopEditor%5B0%5D.ImageFormats%5B2%5D=bmp&DefaultDesktopEditor%5B0%5D.ImageFormats%5B3%5D=tiff&" +
      "DefaultDesktopEditor%5B0%5D.ApplicationPath=test";
    expect(spyFetchPost).toHaveBeenCalledWith(new UrlQuery().UrlApiAppSettings(), query);
  });

  it("Create new item in Array if emthy array", async () => {
    const value = {
      target: { innerText: "test" }
    } as unknown as ChangeEvent<HTMLDivElement>;

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);

    const setIsMessage = jest.fn();
    await UpdateDefaultEditorPhotos(value, setIsMessage, "error_here", "success", [
      {
        applicationPath: "/exist_app",
        imageFormats: [ImageFormat.gif]
      }
    ]);
    expect(setIsMessage).toHaveBeenCalled();
    expect(setIsMessage).toHaveBeenCalledWith("success");

    expect(spyFetchPost).toHaveBeenCalled();

    const query2 =
      "DefaultDesktopEditor%5B0%5D.ImageFormats%5B0%5D=jpg&DefaultDesktopEditor%5B0%5D.ImageFormats%5B1%5D=png&DefaultDesktopEditor" +
      "%5B0%5D.ImageFormats%5B2%5D=bmp&DefaultDesktopEditor%5B0%5D.ImageFormats%5B3%5D=tiff&DefaultDesktopEditor%5B0%5D.ApplicationPath=%2Fexist_app&" +
      "DefaultDesktopEditor%5B1%5D.ImageFormats%5B0%5D=jpg&DefaultDesktopEditor%5B1%5D.ImageFormats%5B1%5D=png&DefaultDesktopEditor%5B1%5D.ImageFormats%5B2%5D=bmp&" +
      "DefaultDesktopEditor%5B1%5D.ImageFormats%5B3%5D=tiff&DefaultDesktopEditor%5B1%5D.ApplicationPath=test";

    expect(spyFetchPost).toHaveBeenCalledWith(new UrlQuery().UrlApiAppSettings(), query2);
  });
});

describe("ToggleCollections", () => {
  it("should call FetchPost with correct URL and bodyParams for toggleCollections function Jpeg", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);
    const appSettings = {
      desktopCollectionsOpen: RawJpegMode.Jpeg,
      useLocalDesktop: true
    } as unknown as IAppSettings;

    await ToggleCollections(true, jest.fn(), "", "", appSettings);

    expect(spyFetchPost).toHaveBeenCalledWith(
      expect.stringContaining(new UrlQuery().UrlApiAppSettings()),
      "desktopCollectionsOpen=2"
    );
  });

  it("should call FetchPost with correct URL and bodyParams for toggleCollections function Raw", async () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 200
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementationOnce(() => mockIConnectionDefault);
    const appSettings = {
      desktopCollectionsOpen: RawJpegMode.Raw,
      useLocalDesktop: true
    } as unknown as IAppSettings;

    await ToggleCollections(false, jest.fn(), "", "", appSettings);

    expect(spyFetchPost).toHaveBeenCalledWith(
      expect.stringContaining(new UrlQuery().UrlApiAppSettings()),
      "desktopCollectionsOpen=1"
    );
  });

  it("ToggleCollections call error if fetchPost is Error 500", async () => {
    const appSettings = {
      desktopCollectionsOpen: RawJpegMode.Default,
      useLocalDesktop: true
    } as unknown as IAppSettings;

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      data: null,
      statusCode: 500
    });

    const spyFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const setIsMessage = jest.fn();
    await ToggleCollections(true, setIsMessage, "error_here", "", appSettings);

    expect(setIsMessage).toHaveBeenCalled();
    expect(setIsMessage).toHaveBeenCalledWith("error_here");

    expect(spyFetchPost).toHaveBeenCalled();
  });
});
