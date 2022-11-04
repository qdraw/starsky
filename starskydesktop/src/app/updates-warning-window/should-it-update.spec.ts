/* eslint-disable @typescript-eslint/ban-types */
import * as appConfig from "electron-settings";
import * as logger from "../logger/logger";
import * as GetNetRequest from "../net-request/get-net-request";
import {
  isPolicyDisabled,
  shouldItUpdate,
  SkipDisplayOfUpdate
} from "./should-it-update";

jest.mock("electron-settings", () => {
  return {
    get: () => Promise.resolve("http://localhost:9609"),
    set: () => "data",
    has: () => true,
    unset: () => {},
    configure: () => {},
    file: () => {},
    __esModule: true,
  };
});

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
    },
    // eslint-disable-next-line object-shorthand, func-names, @typescript-eslint/no-unused-vars
    BrowserWindow: function (_x:object, _y: number, _w: number, _h: number, _s: boolean, _w2: object) {
      return {
        loadFile: jest.fn(),
        once: (_: string, func: Function) => {
          return func();
        },
        show: jest.fn(),
        on: (_: string, func: Function) => {
          return func();
        },
        setMenu: jest.fn(),
        close: jest.fn(),
      };
    },
  };
});

describe("SkipDisplayOfUpdate", () => {
  beforeAll(() => {
    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        info: jest.fn(),
        warn: jest.fn(),
      };
    });
  });

  describe("SkipDisplayOfUpdate", () => {
    //
    it("should skip 1 second ago checked, it should be ignored", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(Date.now().toString());
      });
      const result = await SkipDisplayOfUpdate();
      expect(result).toBeTruthy();
    });
    it("not exist, so it should re-check", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        // eslint-disable-next-line @typescript-eslint/no-unsafe-return
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
    it("is disabled", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });
      const result = await isPolicyDisabled();
      expect(result).toBeTruthy();
      console.log(result);
    });

    it("is not disabled", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });
      const result = await isPolicyDisabled();
      expect(result).toBeFalsy();
      console.log(result);
    });
  });
  describe("shouldItUpdate", () => {
    it("202 is update needed", async () => {
      jest
        .spyOn(GetNetRequest, "GetNetRequest")
        .mockImplementationOnce(() => Promise.resolve({ statusCode: 202 }));

      const result = await shouldItUpdate();
      expect(result).toBeTruthy();
    });
    it("200 is latest version", async () => {
      jest
        .spyOn(GetNetRequest, "GetNetRequest")
        .mockImplementationOnce(() => Promise.resolve({ statusCode: 200 }));

      const result = await shouldItUpdate();
      expect(result).toBeFalsy();
    });
    it("no connection", async () => {
      jest
        .spyOn(GetNetRequest, "GetNetRequest")
        .mockImplementationOnce(() => Promise.reject());

      let errorMessage = null;
      try {
        await shouldItUpdate();
      } catch (error) {
        errorMessage = error as string;
      }
      expect(errorMessage).toBeUndefined();
    });
  });
});
