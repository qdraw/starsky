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
      exposeInMainWorld: (_: string, _2: Function) => {},
    },
    ipcRenderer: {
      send: jest.fn(),
      on: jest.fn(),
    },
  };
});

describe("preload main", () => {
  describe("exposeInMainWorld", () => {
    it("sending valid channel", async () => {
      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      exposeBrigde.send(LocationIsRemoteIpcKey, jest.fn());
      expect(sendSpy).toHaveBeenCalledTimes(1);
      sendSpy.mockReset();
    });

    it("sending non-valid channel", async () => {
      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      exposeBrigde.send("test123", jest.fn());
      expect(sendSpy).toHaveBeenCalledTimes(0);
      sendSpy.mockReset();
    });

    it("sending non-valid channel 1", async () => {
      jest
        .spyOn(contextBridge, "exposeInMainWorld")
        .mockImplementationOnce((name, func) => {
          func.send("test123");
          return null;
        });

      const sendSpy = jest
        .spyOn(ipcRenderer, "send")
        .mockImplementationOnce(() => {
          return null;
        });
      require("./preload-main");

      expect(sendSpy).toHaveBeenCalledTimes(0);
      sendSpy.mockReset();
    });

    it("receive valid channel", async () => {
      const onSpy = jest.spyOn(ipcRenderer, "on").mockImplementationOnce(() => {
        return null;
      });
      exposeBrigde.receive(LocationIsRemoteIpcKey, jest.fn());
      expect(onSpy).toHaveBeenCalledTimes(1);
      onSpy.mockReset();
    });

    it("receive non-valid channel", async () => {
      const onSpy = jest.spyOn(ipcRenderer, "on").mockImplementationOnce(() => {
        return null;
      });
      exposeBrigde.receive("test123", jest.fn());
      expect(onSpy).toHaveBeenCalledTimes(0);
      onSpy.mockReset();
    });
  });
});
