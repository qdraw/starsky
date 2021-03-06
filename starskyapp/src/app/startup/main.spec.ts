import { app, BrowserWindow } from "electron";
import * as setupChildProcess from "../child-process/setup-child-process";
import * as MakeLogsPath from "../config/logs-path";
import * as MakeTempPath from "../config/temp-path";
import * as SetupFileWatcher from "../file-watcher/setup-file-watcher";
import * as ipcBridge from "../ipc-bridge/ipc-bridge";
import * as logger from "../logger/logger";
import * as createMainWindow from "../main-window/create-main-window";
import * as restoreMainWindow from "../main-window/restore-main-window";
import * as AppMenu from "../menu/app-menu";
import * as DockMenu from "../menu/dock-menu";
import * as updatesWarningWindow from "../updates-warning-window/updates-warning-window";
import * as defaultAppSettings from "./app-settings";
import * as willNavigateSecurity from "./will-navigate-security";

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

describe("main", () => {
  const onState = {} as any;

  let setupChildProcessSpy: any = jest.fn();

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

  it("it calling setupChild", () => {
    expect(setupChildProcessSpy).toBeCalled();
  });

  it("should create main window", () => {
    const restoreMainWindowSpy = jest
      .spyOn(restoreMainWindow, "restoreMainWindow")
      .mockImplementationOnce(() => Promise.resolve() as any);
    jest.spyOn(DockMenu, "default").mockImplementationOnce(() => {});
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});

    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      return null;
    });
    onState.ready();
    expect(restoreMainWindowSpy).toBeCalled();
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
      .spyOn(BrowserWindow, "getAllWindows")
      .mockImplementation(() => [] as any);

    jest.spyOn(app, "on").mockImplementation((name: any, func) => {
      if (name === "activate") {
        func();
      }
      return null;
    });

    onState.ready();

    expect(restoreMainWindowSpy).toBeCalled();
    expect(restoreMainWindowSpy).toBeCalledTimes(1);

    expect(createMainWindowSpy).toBeCalled();
    expect(createMainWindowSpy).toBeCalledTimes(1);
  });

  it("when activate and there windows it not should create one", () => {
    jest
      .spyOn(BrowserWindow, "getAllWindows")
      .mockImplementation(() => ["t"] as any);
    jest.spyOn(DockMenu, "default").mockImplementationOnce(() => {});
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});

    const restoreMainWindowSpy = jest
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

    expect(restoreMainWindowSpy).toBeCalled();
    expect(restoreMainWindowSpy).toBeCalledTimes(1);
  });

  describe("platform", () => {
    beforeAll(function () {
      this.originalPlatform = process.platform;
      Object.defineProperty(process, "platform", {
        value: "MockOS"
      });
    });
    afterAll(function () {
      Object.defineProperty(process, "platform", {
        value: this.originalPlatform
      });
    });

    it("window all closed", () => {
      const appQuitSpy = jest
        .spyOn(app, "quit")
        .mockReset()
        .mockImplementation();
      onState.windowallclosed();
      expect(appQuitSpy).toBeCalled();
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
      expect(appQuitSpy).toBeCalledTimes(0);
    });
  });

  it("should call willNavigateSecuritySpy", () => {
    const willNavigateSecuritySpy = jest
      .spyOn(willNavigateSecurity, "willNavigateSecurity")
      .mockImplementationOnce(() => {
        return null;
      });

    const props = {
      on: (_: string, func: Function) => {
        func();
      }
    };
    onState.webcontentscreated("t", props);
    expect(willNavigateSecuritySpy).toBeCalled();
  });
});
