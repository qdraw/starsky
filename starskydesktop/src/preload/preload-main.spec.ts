import { contextBridge, ipcRenderer } from "electron";
import { LocationIsRemoteIpcKey } from "../app/config/location-ipc-keys.const";
import { exposeBrigde } from "./preload-main";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
    },
    contextBridge: {
      // eslint-disable-next-line @typescript-eslint/no-unused-vars, @typescript-eslint/ban-types
      exposeInMainWorld: (_: string, _2: Function) => {},
    },
    ipcRenderer: {
      send: jest.fn(),
      on: jest.fn(),
    },
  };
});
jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("preload main", () => {
  describe("exposeInMainWorld", () => {
    it("sending valid channel", () => {
      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      exposeBrigde.send(LocationIsRemoteIpcKey, jest.fn());
      expect(sendSpy).toHaveBeenCalledTimes(1);
      sendSpy.mockReset();
    });

    it("sending non-valid channel", () => {
      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      exposeBrigde.send("test123", jest.fn());
      expect(sendSpy).toHaveBeenCalledTimes(0);
      sendSpy.mockReset();
    });

    it("sending non-valid channel 1", () => {
      jest
        .spyOn(contextBridge, "exposeInMainWorld")
        .mockImplementationOnce((name, func) => {
          // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
          func.send("test123");
          return null;
        });

      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      // eslint-disable-next-line global-require
      require("./preload-main");

      expect(sendSpy).toHaveBeenCalledTimes(0);
      sendSpy.mockReset();
    });

    it("receive valid channel", () => {
      const onSpy = jest.spyOn(ipcRenderer, "on").mockImplementationOnce(() => {
        return null;
      });
      exposeBrigde.receive(LocationIsRemoteIpcKey, jest.fn());
      expect(onSpy).toHaveBeenCalledTimes(1);
      onSpy.mockReset();
    });

    it("receive non-valid channel", () => {
      const onSpy = jest.spyOn(ipcRenderer, "on").mockImplementationOnce(() => {
        return null;
      });
      exposeBrigde.receive("test123", jest.fn());
      expect(onSpy).toHaveBeenCalledTimes(0);
      onSpy.mockReset();
    });
  });
});
