/* eslint-disable prefer-promise-reject-errors */
/* eslint-disable jest/no-done-callback */
/* eslint-disable jest/no-conditional-expect */
/* eslint-disable @typescript-eslint/ban-types */
import BrowserWindow from "electron";
import * as appConfig from "electron-settings";
import * as logger from "../logger/logger";
import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
import * as shouldItUpdate from "./should-it-update";
import createCheckForUpdatesContainerWindow, {
  checkForUpdatesWindow
} from "./updates-warning-window";

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

const mockBrowserWindow = {
  loadFile: jest.fn(),
  once: (_: string, func: Function) => {
    return func();
  },
  show: jest.fn(),
  on: (_: string, func: Function) => {
    return func();
  },
  setMenu: jest.fn(),
  __esModule: true,
};

const mockWindowStateKeeper = {
  x: 0,
  y: 0,
  width: 1,
  height: 1,
  isMaximized: false,
  track: jest.fn()
};

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
      __esModule: true,
    },
    // eslint-disable-next-line object-shorthand, func-names, @typescript-eslint/no-unused-vars
    BrowserWindow: function (_x:object, _y: number, _w: number, _h: number, _s: boolean, _w2: object) {
      return mockBrowserWindow;
    }
  };
});

describe("create main window", () => {
  beforeAll(() => {
    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        warn: jest.fn(),
        info: jest.fn()
      };
    });
  });

  describe("checkForUpdatesWindow", () => {
    it("should call browserWindow", async () => {
      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));
      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      jest
        .spyOn(appConfig, "set")
        .mockImplementationOnce(() => Promise.resolve())
        .mockImplementationOnce(() => Promise.resolve());

      await checkForUpdatesWindow();

      expect(browserWindowSpy).toHaveBeenCalled();
    });
  });
  describe("createCheckForUpdatesContainerWindow", () => {
    // eslint-disable-next-line jest/no-done-callback
    it("should call browserWindow", (done: Function) => {
      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));

      jest
        .spyOn(shouldItUpdate, "isPolicyDisabled")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "SkipDisplayOfUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "shouldItUpdate")
        .mockImplementationOnce(() => Promise.resolve(true));

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          // eslint-disable-next-line jest/no-conditional-expect
          expect(browserWindowSpy).toHaveBeenCalled();
          done();
        })
        .catch((e) => {
          console.log(e);
          throw e;
        });
    });
    // eslint-disable-next-line jest/no-done-callback
    it("should not call browserWindow", (done : Function) => {
      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));

      jest
        .spyOn(shouldItUpdate, "isPolicyDisabled")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "SkipDisplayOfUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "shouldItUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow").mockClear()
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          expect(browserWindowSpy).toHaveBeenCalledTimes(0);
          done();
        })
        .catch((e) => {
          console.log(e);
          throw e;
        });
    });

    it("should reject when feature toggle is disabled", (done) => {
      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));

      jest
        .spyOn(shouldItUpdate, "isPolicyDisabled")
        .mockImplementationOnce(() => Promise.resolve(true));

      jest
        .spyOn(shouldItUpdate, "SkipDisplayOfUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "shouldItUpdate")
        .mockImplementationOnce(() => Promise.resolve(true));

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          // it should never call this 0>
          expect(true).toBeFalsy();
        })
        .catch((e) => {
          expect(browserWindowSpy).toHaveBeenCalledTimes(0);
          done();
        });
    });

    it("should reject when shouldItUpdate fails", (done: Function) => {
      jest.spyOn(logger, "default").mockImplementation(() => {
        return {
          warn: jest.fn()
        };
      });
      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));

      jest
        .spyOn(shouldItUpdate, "isPolicyDisabled")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "SkipDisplayOfUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest.spyOn(shouldItUpdate, "shouldItUpdate").mockReset();

      jest
        .spyOn(shouldItUpdate, "shouldItUpdate")
        .mockImplementationOnce(() => Promise.reject(true));

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then((e) => {
          // it should never call this 0>
          // eslint-disable-next-line jest/no-conditional-expect
          expect(true).toBeFalsy();
        })
        .catch((e) => {
          // eslint-disable-next-line jest/no-conditional-expect
          expect(browserWindowSpy).toHaveBeenCalledTimes(0);
          done();
        });
    });

    it("should not be undefined", () => {
      jest
        .spyOn(shouldItUpdate, "isPolicyDisabled")
        .mockImplementationOnce(() => Promise.resolve(false));

      jest
        .spyOn(shouldItUpdate, "SkipDisplayOfUpdate")
        .mockImplementationOnce(() => Promise.resolve(false));

      const result = createCheckForUpdatesContainerWindow();
      expect(result).toBeDefined();
    });
  });
});
