/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable global-require */
import { app, BrowserWindow } from "electron";
import * as setupChildProcess from "../app/child-process/setup-child-process";
import * as MakeLogsPath from "../app/config/logs-path";
import * as MakeTempPath from "../app/config/temp-path";
import * as SetupFileWatcher from "../app/file-watcher/setup-file-watcher";
import * as ipcBridge from "../app/ipc-bridge/ipc-bridge";
import * as logger from "../app/logger/logger";
import * as createMainWindow from "../app/main-window/create-main-window";
import * as restoreMainWindow from "../app/main-window/restore-main-window";
import * as AppMenu from "../app/menu/app-menu";
import * as DockMenu from "../app/menu/dock-menu";
import * as defaultAppSettings from "../app/startup/app-settings";
import * as RestoreWarmupMainWindowAndCloseSplash from "../app/startup/restore-warmup-main-window-and-close-splash";
import * as willNavigateSecurity from "../app/startup/will-navigate-security";
import * as updatesWarningWindow from "../app/updates-warning-window/updates-warning-window";
import * as IsRemote from "../app/warmup/is-remote";
import * as SetupSplash from "../app/warmup/splash";
import * as WarmupServer from "../app/warmup/warmup-server";

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

const mockBrowserWindow = {
  loadFile: jest.fn(),
  // eslint-disable-next-line @typescript-eslint/ban-types
  once: (_: string, func: Function) => {
    return func();
  },
  show: jest.fn(),
  // eslint-disable-next-line @typescript-eslint/ban-types
  on: (_: string, func: Function) => {
    return func();
  },
  getAllWindows: [],
  setMenu: jest.fn(),
  __esModule: true,
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

describe("main", () => {
  const onState = {} as any;

  let setupChildProcessSpy: any = jest.fn();
  let restoreWarmupMainWindowAndCloseSplashSpy: any = jest.fn();
  let isRemoteSpy: any = jest.fn();

  beforeAll(() => {
    jest
      .spyOn(updatesWarningWindow, "default")
      .mockImplementationOnce(() => Promise.resolve(true));
    jest.spyOn(ipcBridge, "default").mockImplementationOnce(() => {});
    jest
      .spyOn(defaultAppSettings, "default")
      .mockImplementationOnce(() => "test");

    setupChildProcessSpy = jest
      .spyOn(setupChildProcess, "setupChildProcess")
      .mockImplementationOnce(() => Promise.resolve());
    jest
      .spyOn(MakeTempPath, "MakeTempPath")
      .mockImplementationOnce(() => "test");
    jest
      .spyOn(MakeLogsPath, "MakeLogsPath")
      .mockImplementationOnce(() => "test");

    jest
      .spyOn(SetupFileWatcher, "SetupFileWatcher")
      .mockImplementationOnce(() => Promise.resolve());

    restoreWarmupMainWindowAndCloseSplashSpy = jest.spyOn(
      RestoreWarmupMainWindowAndCloseSplash,
      "default"
    ).mockImplementationOnce(() => {});

    isRemoteSpy = jest.spyOn(IsRemote, "IsRemote").mockImplementationOnce(() => {
      return Promise.resolve(false);
    });

    jest
      .spyOn(SetupSplash, "SetupSplash")
      .mockImplementationOnce(() => {
        return {} as any;
      });

    jest
      .spyOn(SetupSplash, "CloseSplash")
      .mockImplementationOnce(() => {
        return {
          close: jest.fn()
        } as any;
      });

    jest
      .spyOn(WarmupServer, "WarmupServer")
      .mockImplementationOnce(() => Promise.resolve(true));

    // this excuted only once
    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      onState[name.replace(/-/gi, "")] = func;
      console.log(name.replace(/-/gi, ""), func);
      return null;
    });

    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        info: jest.fn(),
        warn: jest.fn()
      };
    });

    require("./main");
  });

  it("calling setupChild", () => {
    expect(setupChildProcessSpy).toHaveBeenCalled();
  });

  it("should create main window", () => {
    jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve() as any);
    jest.spyOn(DockMenu, "default").mockImplementationOnce(() => {});
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});

    jest
      .spyOn(SetupSplash, "SetupSplash")
      .mockImplementationOnce(() => {
        return {} as any;
      });

    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      return null;
    });

    onState.ready();
    expect(isRemoteSpy).toHaveBeenCalled();
  });

  it("when activate and there a no windows it should create one", () => {
    jest
      .spyOn(updatesWarningWindow, "default")
      .mockImplementationOnce(() => Promise.resolve(true));

    const restoreMainWindowSpy = jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve() as any);
    jest.spyOn(DockMenu, "default").mockImplementationOnce(() => {});
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve() as any);

    jest
      .spyOn(SetupSplash, "SetupSplash")
      .mockImplementationOnce(() => Promise.resolve({}) as any);

    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      if (name === "activate") {
        func();
      }
      return null;
    });

    onState.ready();

    expect(restoreWarmupMainWindowAndCloseSplashSpy).toHaveBeenCalled();
    expect(restoreWarmupMainWindowAndCloseSplashSpy).toHaveBeenCalledTimes(1);
  });

  it("when activate and there windows it not should create one", () => {
    jest
      .spyOn(BrowserWindow, "getAllWindows")
      .mockImplementation(() => ["t"] as any);
    jest.spyOn(DockMenu, "default").mockImplementationOnce(() => {});
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});

    jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve() as any);

    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      if (name === "activate") {
        func();
      }
      return null;
    });
    onState.ready();

    expect(restoreWarmupMainWindowAndCloseSplashSpy).toHaveBeenCalled();
    expect(restoreWarmupMainWindowAndCloseSplashSpy).toHaveBeenCalledTimes(1);
  });

  let originalPlatform = process.platform;
  describe("platform", () => {
    beforeAll(() => {
      originalPlatform = process.platform;
      Object.defineProperty(process, "platform", {
        value: "MockOS"
      });
    });
    afterAll(() => {
      Object.defineProperty(process, "platform", {
        value: originalPlatform
      });
    });

    it("window all closed", () => {
      const appQuitSpy = jest
        .spyOn(app, "quit")
        .mockReset()
        .mockImplementation();
      onState.windowallclosed();
      expect(appQuitSpy).toHaveBeenCalled();
    });

    it("window all closed (but not for mac os)", () => {
      const appQuitSpy = jest
        .spyOn(app, "quit")
        .mockReset()
        .mockImplementation();
      Object.defineProperty(process, "platform", {
        value: "darwin"
      });
      onState.windowallclosed();
      expect(appQuitSpy).toHaveBeenCalledTimes(0);
    });
  });

  it("should call willNavigateSecuritySpy", () => {
    const willNavigateSecuritySpy = jest
      .spyOn(willNavigateSecurity, "willNavigateSecurity")
      .mockImplementationOnce(() => {
        return null;
      });

    const props = {
      // eslint-disable-next-line @typescript-eslint/ban-types
      on: (_: string, func: Function) => {
        func();
      }
    };
    onState.webcontentscreated("t", props);
    expect(willNavigateSecuritySpy).toHaveBeenCalled();
  });
});
