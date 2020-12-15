import * as BrowserWindow from "electron";
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
    xit("should call browserWindow", async () => {
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
  });
});
