import * as appConfig from "electron-settings";
import { unfreezeImport } from "../../shared/unfreeze-import";
import { GetBaseUrlFromSettings } from "./get-base-url-from-settings";

unfreezeImport(appConfig, 'get');

jest.mock('net', () => ({
  Socket() {
    return {
      connect() {
        return 'Hello World';
      },
    };
  },
}));

jest.mock("electron", () => {
  return {
    app: {
      getPath: () => "tmp",
    },
  };
});

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

    it("set remote to false but no url", async () => {
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("false");
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("test.com/1");
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
