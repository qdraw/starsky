import * as RestoreMainWindow from "../main-window/restore-main-window";
import * as WarmupServer from "../warmup/warmup-server";
import * as RestoreWarmupMainWindowAndCloseSplash from "./restore-warmup-main-window-and-close-splash";

jest.mock("electron", () => {
  return {
    app: {
      allowRendererProcessReuse: true,
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
      quit: () => "en",
      getName: () => "test",
    },
    ipcMain: {
      on: () => "en",
    },
    BrowserWindow: {
      id: 801,
      // eslint-disable-next-line @typescript-eslint/no-unsafe-return
      getAllWindows: () => [] as any[],
    },
  };
});

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("RestoreWarmupMainWindowAndCloseSplash", () => {
  it("should close splash window", () => {
    const window = [
      {
        close: jest.fn(),
      },
    ];

    const restoreMainWindowSpy = jest
      .spyOn(RestoreMainWindow, "RestoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve());

    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    RestoreWarmupMainWindowAndCloseSplash.default(1, window as any, true);

    expect(window[0].close).toHaveBeenCalled();
    expect(restoreMainWindowSpy).toHaveBeenCalled();
  });

  it("should close splash window 2", () => {
    const window = [
      {
        close: jest.fn(),
      },
    ];

    const warmupServerSpy = jest
      .spyOn(WarmupServer, "WarmupServer")
      .mockImplementationOnce(() => {
        return Promise.resolve(true);
      });

    const restoreMainWindowSpy = jest
      .spyOn(RestoreMainWindow, "RestoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve());

    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    RestoreWarmupMainWindowAndCloseSplash.default(1, window as any, false);

    expect(warmupServerSpy).toHaveBeenCalled();
    expect(restoreMainWindowSpy).toHaveBeenCalled();
  });
});
