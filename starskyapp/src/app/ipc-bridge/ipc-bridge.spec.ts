import { net } from "electron";
import * as appConfig from "electron-settings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../config/location-ipc-keys.const";
import {
  AppVersionCallback,
  LocationIsRemoteCallback,
  LocationUrlCallback
} from "./ipc-bridge";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en"
    },
    net: {
      request: () => {}
    }
  };
});

describe("ipc bridge", () => {
  describe("LocationIsRemoteCallback", () => {
    it("getting with null input", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });
      await LocationIsRemoteCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationIsRemoteIpcKey, false);
    });

    it("set to true", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await LocationIsRemoteCallback(event, true);

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationIsRemoteIpcKey, true);
    });

    it("set to false", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await LocationIsRemoteCallback(event, false);

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationIsRemoteIpcKey, false);
    });
  });

  describe("AppVersionCallback", () => {
    it("check if version has output", async () => {
      const event = { reply: jest.fn() } as any;
      AppVersionCallback(event);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(AppVersionIpcKey, ["99.99"]);
    });
  });

  describe("LocationUrlCallback", () => {
    it("get remote is off", async () => {
      const event = { reply: jest.fn() } as any;
      // is remote?
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });
      await LocationUrlCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: true,
        isValid: null,
        location: "http://localhost:9609"
      });
    });

    it("get remote is on but no url so its off", async () => {
      jest.spyOn(appConfig, "get").mockReset();
      const event = { reply: jest.fn() } as any;
      // is remote?
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });
      await LocationUrlCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: true,
        isValid: null,
        location: "http://localhost:9609"
      });
    });

    it("getting remote is on but no url so its off", async () => {
      const event = { reply: jest.fn() } as any;
      // is remote?
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve("__url_from_config__");
      });

      await LocationUrlCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: null,
        location: "__url_from_config__"
      });
    });

    it("update non valid url", async () => {
      const event = { reply: jest.fn() } as any;

      await LocationUrlCallback(event, "__url_from_params__");
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: false,
        location: "__url_from_params__"
      });
    });

    it("update valid url", async () => {
      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                headers: { test: true },
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });
      const event = { reply: jest.fn() } as any;

      await LocationUrlCallback(event, "https://google.com");

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: true,
        location: "https://google.com"
      });
    });

    it("update failed url", async () => {
      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                headers: { test: true },
                statusCode: 900
              });
            }
          },
          end: jest.fn()
        } as any;
      });
      const event = { reply: jest.fn() } as any;

      await LocationUrlCallback(event, "https://fail.com");

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: false,
        location: "https://fail.com"
      });
    });

    it("non valid domain", async () => {
      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "error") {
              fun();
            }
          },
          end: jest.fn()
        } as any;
      });
      const event = { reply: jest.fn() } as any;

      await LocationUrlCallback(event, "https://__non_exiting_domain");

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: false,
        location: "https://__non_exiting_domain"
      });
    });
  });
});
