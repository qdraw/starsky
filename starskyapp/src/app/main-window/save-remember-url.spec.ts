import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import { removeRememberUrl, saveRememberUrl } from "./save-remember-url";

describe("save remember url", () => {
  describe("saveRememberUrl", () => {
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

      expect(appSettingsSetSpy).toHaveBeenCalled();
      expect(appSettingsSetSpy).toHaveBeenCalledWith(RememberUrl, { 1: "/?f=t" });
    });

    it("should merge", async () => {
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

      expect(appSettingsSetSpy).toHaveBeenCalled();
      expect(appSettingsSetSpy).toHaveBeenCalledWith(RememberUrl, {
        0: "test",
        1: "/?f=t"
      });
    });

    it("should ignore file://", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(undefined);
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
            return "file://";
          }
        }
      } as any);

      expect(appSettingsSetSpy).toHaveBeenCalledTimes(0);
    });
  });
  describe("removeRememberUrl", () => {
    it("should remove it from list", async () => {
      jest
        .spyOn(appConfig, "get")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve({ 0: "test" });
        });

      const appSettingsSetSpy = jest
        .spyOn(appConfig, "set")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve(undefined);
        });

      await removeRememberUrl(0);
      expect(appSettingsSetSpy).toHaveBeenCalled();
      expect(appSettingsSetSpy).toHaveBeenCalledWith(RememberUrl, {});
    });

    it("should not existing it from list", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve({});
      });

      const appSettingsSetSpy = jest
        .spyOn(appConfig, "set")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve(undefined);
        });

      await removeRememberUrl(0);
      expect(appSettingsSetSpy).toHaveBeenCalled();
      expect(appSettingsSetSpy).toHaveBeenCalledWith(RememberUrl, {});
    });
  });
});
