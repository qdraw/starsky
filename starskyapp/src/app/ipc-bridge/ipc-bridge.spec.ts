import { net } from "electron";
import * as appConfig from "electron-settings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import { DefaultImageApplicationIpcKey } from "../config/default-image-application-settings-ipc-key.const";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../config/location-ipc-keys.const";
import { UpdatePolicyIpcKey } from "../config/update-policy-ipc-key.const";
import * as fileSelectorWindow from "../file-selector-window/file-selector-window";
import * as SetupFileWatcher from "../file-watcher/setup-file-watcher";
import * as logger from "../logger/logger";
import * as createMainWindow from "../main-window/create-main-window";
import { mainWindows } from "../main-window/main-windows.const";
import {
  AppVersionCallback,
  DefaultImageApplicationCallback,
  LocationIsRemoteCallback,
  LocationUrlCallback,
  UpdatePolicyCallback
} from "./ipc-bridge";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    net: {
      request: () => {}
    }
  };
});

describe("ipc bridge", () => {
  beforeAll(() => {
    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        warn: jest.fn(),
        info: jest.fn()
      };
    });
  });
  describe("LocationIsRemoteCallback", () => {
    it("getting with null input (LocationIsRemoteCallback)", async () => {
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

      jest
        .spyOn(appConfig, "set")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        })
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      jest
        .spyOn(createMainWindow, "default")
        .mockImplementationOnce(() =>
          Promise.resolve({ once: jest.fn() } as any)
        );

      jest
        .spyOn(SetupFileWatcher, "SetupFileWatcher")
        .mockImplementationOnce(() => Promise.resolve());

      await LocationIsRemoteCallback(event, true);

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationIsRemoteIpcKey, true);
    });

    it("set to false", async () => {
      const event = { reply: jest.fn() } as any;

      jest
        .spyOn(appConfig, "set")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      jest
        .spyOn(createMainWindow, "default")
        .mockImplementationOnce(() =>
          Promise.resolve({ once: jest.fn() } as any)
        );

      jest
        .spyOn(SetupFileWatcher, "SetupFileWatcher")
        .mockImplementationOnce(() => Promise.resolve());

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
      jest.spyOn(logger, "default").mockRestore();

      jest
        .spyOn(appConfig, "set")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve();
        })
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      jest
        .spyOn(createMainWindow, "default")
        .mockImplementationOnce(() =>
          Promise.resolve({ once: jest.fn() } as any)
        );

      jest
        .spyOn(SetupFileWatcher, "SetupFileWatcher")
        .mockImplementationOnce(() => Promise.resolve());

      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log("valid url ");
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("{}");
                    return;
                  }
                  func();
                  console.log(param);
                },
                headers: {},
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });

      const event = { reply: jest.fn() } as any;

      mainWindows.add({ close: jest.fn() } as any);

      console.log("update valid url");

      await LocationUrlCallback(event, "https://google.com");

      console.log("update valid url ----- <<<<<< ");

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: true,
        location: "https://google.com"
      });

      mainWindows.clear();
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
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("{}");
                    return;
                  }
                  func();
                  console.log(param);
                },
                headers: {},
                statusCode: 500
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
      console.log("--end fail.com");
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

      await LocationUrlCallback(event, "https://nonexitingdomain.com");

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(LocationUrlIpcKey, {
        isLocal: false,
        isValid: false,
        location: "https://nonexitingdomain.com",
        reason: {
          error: undefined,
          statusCode: 999
        }
      });
    });
  });

  describe("UpdatePolicyCallback", () => {
    it("getting with null input (UpdatePolicyCallback)", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });
      await UpdatePolicyCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(UpdatePolicyIpcKey, true);
    });

    it("set to true", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await UpdatePolicyCallback(event, true);

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(UpdatePolicyIpcKey, true);
    });

    it("set to false", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await UpdatePolicyCallback(event, false);

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(UpdatePolicyIpcKey, false);
    });
  });

  describe("DefaultImageApplicationCallback", () => {
    it("getting with null input (DefaultImageApplicationCallback)", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockReset();
      jest
        .spyOn(appConfig, "get")
        .mockImplementationOnce(() => Promise.resolve(null));
      await DefaultImageApplicationCallback(event, null);
      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(DefaultImageApplicationIpcKey, null);
    });

    it("set reset of DefaultImageApplicationCallback", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await DefaultImageApplicationCallback(event, { reset: true });

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(DefaultImageApplicationIpcKey, false);
    });

    it("should give successfull showOpenDialog", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest
        .spyOn(fileSelectorWindow, "fileSelectorWindow")
        .mockImplementationOnce(() =>
          Promise.resolve(["result_from_fileSelectorWindow"])
        );

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await DefaultImageApplicationCallback(event, { showOpenDialog: true });

      expect(event.reply).toBeCalled();
      expect(event.reply).toBeCalledWith(
        DefaultImageApplicationIpcKey,
        "result_from_fileSelectorWindow"
      );
    });

    it("should ignore failing showOpenDialog", async () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

      jest
        .spyOn(fileSelectorWindow, "fileSelectorWindow")
        .mockImplementationOnce(() =>
          Promise.reject(["result_from_fileSelectorWindow"])
        );

      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      await DefaultImageApplicationCallback(event, {
        showOpenDialog: true
      });

      expect(event.reply).toBeCalledTimes(0);
    });
  });
});
