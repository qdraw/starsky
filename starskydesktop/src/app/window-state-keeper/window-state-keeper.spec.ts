import * as appConfig from "electron-settings";
import { windowStateKeeper } from "./window-state-keeper";

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
  };
});

describe("window state keeper", () => {
  describe("windowStateKeeper", () => {
    it("windowStateKeeper default output", async () => {
      jest.spyOn(appConfig, "has").mockClear();
      jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
        return Promise.resolve(false);
      });
      const result = await windowStateKeeper("test3");
      expect(result.x).toBeUndefined();
      expect(result.y).toBeUndefined();
      expect(result.width).toBe(1000);
      expect(result.height).toBe(800);
    });

    it("default output 2", async () => {
      jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve({
          x: 10,
          y: 11,
          width: 12,
          height: 13,
        });
      });
      const result = await windowStateKeeper("test");
      expect(result.x).toBe(10);
      expect(result.y).toBe(11);
      expect(result.width).toBe(12);
      expect(result.height).toBe(13);
    });

    it("on change it should see callback", async () => {
      const result = await windowStateKeeper("test");
      const onMock = jest.fn();
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      result.track({
        resize: jest.fn(),
        move: jest.fn(),
        close: jest.fn(),
        on: onMock,
      } as any);

      expect(onMock).toHaveBeenCalled();
      expect(onMock).toHaveBeenCalledWith("resize", expect.anything());
      expect(onMock).toHaveBeenNthCalledWith(1, "resize", expect.anything());
      expect(onMock).toHaveBeenNthCalledWith(2, "move", expect.anything());
      expect(onMock).toHaveBeenNthCalledWith(3, "close", expect.anything());
    });

    it("trigger onSave callback Maximized", async () => {
      jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve({
          x: 10,
          y: 11,
          width: 12,
          height: 13,
          isMaximized: false,
        });
      });
      const appConfigSetSpy = jest
        .spyOn(appConfig, "set")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      const result = await windowStateKeeper("test");
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      result.track({
        resize: jest.fn(),
        move: jest.fn(),
        close: jest.fn(),
        getBounds: () => ({
          x: 10,
          y: 11,
          width: 12,
          height: 13,
          isMaximized: false,
        }),
        isMaximized: () => true,

        // eslint-disable-next-line @typescript-eslint/ban-types
        on: (event: any, callback: Function) => {
          if (event === "resize") {
            callback();
          }
        },
      } as any);

      expect(appConfigSetSpy).toHaveBeenCalled();
      expect(appConfigSetSpy).toHaveBeenCalledWith("windowState.test", {
        height: 40,
        isMaximized: true,
        width: 20,
        x: 10,
        y: 11,
      });
    });

    it("trigger onSave min sizes callback Maximized", async () => {
      jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
        return Promise.resolve(true);
      });
      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve({
          x: 10,
          y: 11,
          width: 41,
          height: 41,
          isMaximized: false,
        });
      });
      const appConfigSetSpy = jest
        .spyOn(appConfig, "set")
        .mockClear()
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      const result = await windowStateKeeper("test");
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      result.track({
        resize: jest.fn(),
        move: jest.fn(),
        close: jest.fn(),
        getBounds: () => ({
          x: 10,
          y: 11,
          width: 12,
          height: 13,
          isMaximized: false,
        }),
        isMaximized: () => true,

        // eslint-disable-next-line @typescript-eslint/ban-types
        on: (event: any, callback: Function) => {
          if (event === "resize") {
            callback();
          }
        },
      } as any);

      expect(appConfigSetSpy).toHaveBeenCalled();
      expect(appConfigSetSpy).toHaveBeenCalledWith("windowState.test", {
        height: 40,
        isMaximized: true,
        width: 20,
        x: 10,
        y: 11,
      });
    });
  });
});
