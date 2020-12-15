import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import { saveRememberUrl } from "./save-remember-url";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    BrowserWindow: () => {
      return {
        loadFile: jest.fn(),
        id: 0,
        webContents: {
          userAgent: "test",
          on: (_: string, func: Function) => {
            return func();
          },
          getURL: () => {}
        },
        once: (_: string, func: Function) => {
          return func();
        },
        show: jest.fn(),
        on: (_: string, func: Function) => {
          return func();
        }
      };
    }
  };
});

describe("save remember url", () => {
  it("new situation", async () => {
    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve(undefined);
    });

    const appSettingsSetSpy = jest
      .spyOn(appConfig, "set")
      .mockImplementationOnce(() => {
        return Promise.resolve(undefined);
      });

    await saveRememberUrl({
      id: 1,
      webContents: {
        getURL: () => {
          return "https://google.com/?f=t";
        }
      }
    } as any);

    //
    expect(appSettingsSetSpy).toBeCalled();
    expect(appSettingsSetSpy).toBeCalledWith(RememberUrl, { "1": "?f=t" });
  });
});
