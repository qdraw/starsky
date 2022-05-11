
import * as restoreMainWindow from "../main-window/restore-main-window";
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
      getName: () => "test"
    },
    ipcMain: {
      on: () => "en"
    },
    BrowserWindow: {
      id: 801,
      getAllWindows: () => [] as any[]
    }
  };
});


describe("RestoreWarmupMainWindowAndCloseSplash", () => {
  it("should close splash window", () => {

    const window = {
      close: jest.fn()
    }

    const restoreMainWindowSpy = jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve());

    RestoreWarmupMainWindowAndCloseSplash.default(window as any, true);

    expect(window.close).toBeCalled()
            expect(restoreMainWindowSpy).toBeCalled()
  });

    it("should close splash window 2", () => {

    const window = {
      close: jest.fn()
    }

    const warmupServerSpy = jest.spyOn(WarmupServer, "WarmupServer").mockImplementationOnce(()=>{
      return Promise.resolve(true)
    })

    const restoreMainWindowSpy = jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve());

    RestoreWarmupMainWindowAndCloseSplash.default(window as any, false);

    expect(warmupServerSpy).toBeCalled()
    expect(restoreMainWindowSpy).toBeCalled()
  });
});