import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import { saveRememberUrl } from "./save-remember-url";

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

    expect(appSettingsSetSpy).toBeCalled();
    expect(appSettingsSetSpy).toBeCalledWith(RememberUrl, { "1": "?f=t" });
  });

  it("should merge ", async () => {
    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve({ 0: "test" });
    });

    const appSettingsSetSpy = jest
      .spyOn(appConfig, "set")
      .mockReset()
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

    expect(appSettingsSetSpy).toBeCalled();
    expect(appSettingsSetSpy).toBeCalledWith(RememberUrl, { "1": "?f=t" });
  });
});
