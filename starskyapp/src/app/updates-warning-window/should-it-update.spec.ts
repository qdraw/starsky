import * as appConfig from "electron-settings";
import * as logger from "../logger/logger";
import * as GetNetRequest from "../net-request/get-net-request";
import {
  isPolicyDisabled,
  shouldItUpdate,
  SkipDisplayOfUpdate
} from "./should-it-update";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    }
  };
});

describe("SkipDisplayOfUpdate", () => {
  beforeAll(() => {
    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        info: jest.fn(),
        warn: jest.fn()
      };
    });
  });

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
    it("no connection", async (done) => {
      jest
        .spyOn(GetNetRequest, "GetNetRequest")
        .mockImplementationOnce(() => Promise.reject());

      shouldItUpdate()
        .catch((result) => {
          expect(result).toBeUndefined();
          done();
        })
        .then(() => {
          new Error("should return catch");
        });
    });
  });
});
