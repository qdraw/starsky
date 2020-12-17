import * as appConfig from "electron-settings";
import defaultAppSettings from "./app-settings";

describe("app settings", () => {
  describe("defaultAppSettings", () => {
    it("default output", async () => {
      jest.spyOn(appConfig, "configure").mockImplementationOnce(() => {});
      jest.spyOn(appConfig, "file").mockImplementationOnce(() => "test");

      const result = defaultAppSettings();
      expect(result).toBe("test");
    });
  });
});
