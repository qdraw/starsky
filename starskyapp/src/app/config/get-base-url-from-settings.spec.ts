import * as appConfig from "electron-settings";
import { GetBaseUrlFromSettings } from "./get-base-url-from-settings";

describe("GetBaseUrlFromSettings", () => {
  describe("GetBaseUrlFromSettings", () => {
    it("nothing saved", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });

      const result = await GetBaseUrlFromSettings();
      expect(result.location).toBe("http://localhost:9609");
    });
    it("set remote to true but no url", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("true");
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });

      const result = await GetBaseUrlFromSettings();
      expect(result.location).toBe("http://localhost:9609");
    });

    it("set remote to true and have url", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("true");
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("test.com");
      });

      const result = await GetBaseUrlFromSettings();
      expect(result.location).toBe("test.com");
    });
  });
});
