import * as appConfig from "electron-settings";
import { isPolicyEnabled, SkipDisplayOfUpdate } from "./updates-warning-window";

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

describe("create main window", () => {
  describe("SkipDisplayOfUpdate", () => {
    it("should skip 1 second ago checked, it should be ignored", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(Date.now().toString());
      });
      const result = await SkipDisplayOfUpdate();
      expect(result).toBeTruthy();
    });
    it("not exist, so it should re-check", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });
      const result = await SkipDisplayOfUpdate();
      expect(result).toBeFalsy();
    });

    it("has invalid config stored, so it should re-check", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("NaN");
      });
      const result = await SkipDisplayOfUpdate();
      expect(result).toBeFalsy();
    });

    it("long time ago, so it should re-check", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("150000");
      });
      const result = await SkipDisplayOfUpdate();
      expect(result).toBeFalsy();
    });
  });
  describe("isPolicyEnabled", () => {
    it("disabled", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("false");
      });
      const result = await isPolicyEnabled();
      expect(result).toBeFalsy();
      console.log(result);
    });
    it("disabled", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("true");
      });
      const result = await isPolicyEnabled();
      expect(result).toBeFalsy();
      console.log(result);
    });
  });
});
