import * as BrowserWindow from "electron";
import * as appConfig from "electron-settings";
import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
import * as shouldItUpdate from "./should-it-update";
import createCheckForUpdatesContainerWindow, {
  checkForUpdatesWindow
} from "./updates-warning-window";

const mockBrowserWindow = {
  loadFile: jest.fn(),
  once: (_: string, func: Function) => {
    return func();
  },
  show: jest.fn(),
  on: (_: string, func: Function) => {
    return func();
  },
  setMenu: jest.fn()
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
      on: () => "en"
    },
    BrowserWindow: () => mockBrowserWindow
  };
});

describe("create main window", () => {
  describe("checkForUpdatesWindow", () => {
    it("should call browserWindow", async () => {
      jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      jest
        .spyOn(windowStateKeeper, "windowStateKeeper")
        .mockImplementationOnce(() => Promise.resolve(mockWindowStateKeeper));
      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      await checkForUpdatesWindow();

      expect(browserWindowSpy).toBeCalled();
    });
  });
  describe("createCheckForUpdatesContainerWindow", () => {
    it("should call browserWindow", (done) => {
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
        .mockReset()
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          expect(browserWindowSpy).toBeCalled();
          done();
        })
        .catch((e) => {
          console.log(e);
          throw e;
        });
    });
    it("should not call browserWindow", (done) => {
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
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockReset()
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          expect(browserWindowSpy).toBeCalledTimes(0);
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

      jest.spyOn(BrowserWindow, "BrowserWindow").mockReset();

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then(() => {
          // it should never call this 0>
          expect(true).toBeFalsy();
        })
        .catch((e) => {
          expect(browserWindowSpy).toBeCalledTimes(0);
          done();
        });
    });

    it("should reject when shouldItUpdate fails", (done) => {
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

      jest.spyOn(BrowserWindow, "BrowserWindow").mockReset();

      const browserWindowSpy = jest
        .spyOn(BrowserWindow, "BrowserWindow")
        .mockImplementationOnce(() => mockBrowserWindow as any);

      createCheckForUpdatesContainerWindow(1)
        .then((e) => {
          // it should never call this 0>
          expect(true).toBeFalsy();
        })
        .catch((e) => {
          expect(browserWindowSpy).toBeCalledTimes(0);
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
      expect(result).not.toBeUndefined();
    });
  });
});
