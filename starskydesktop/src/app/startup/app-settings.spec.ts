import * as appConfig from "electron-settings";
import * as logger from "../logger/logger";
import defaultAppSettings from "./app-settings";

jest.mock("electron-settings", () => {
  return {
    get: () => "http://localhost:9609",
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
      getPath: () => "tmp",
    },
  };
});

describe("app settings", () => {
  beforeEach(() => {
    jest.spyOn(logger, "default").mockRestore();
  });
  describe("defaultAppSettings", () => {
    it("default output", () => {
      jest.spyOn(appConfig, "configure").mockImplementationOnce(() => {});
      jest.spyOn(appConfig, "file").mockImplementationOnce(() => "test");

      const result = defaultAppSettings();
      expect(result).toBe("test");
    });
  });
});
