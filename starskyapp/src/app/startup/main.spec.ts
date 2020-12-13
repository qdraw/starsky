import * as setupChildProcess from "../child-process/setup-child-process";
import * as ipcBridge from "../ipc-bridge/ipc-bridge";
import * as AppMenu from "../menu/menu";
import * as defaultAppSettings from "./app-settings";

jest.mock("electron", () => {
  return {
    app: {
      allowRendererProcessReuse: true,
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    ipcMain: {
      on: jest.fn()
    }
  };
});

describe("main", () => {
  it("it calling setupChild", async () => {
    jest.spyOn(ipcBridge, "default").mockImplementationOnce(() => {});
    jest
      .spyOn(defaultAppSettings, "default")
      .mockImplementationOnce(() => "test");
    jest.spyOn(AppMenu, "default").mockImplementationOnce(() => {});
    const setupChildProcessSpy = jest
      .spyOn(setupChildProcess, "setupChildProcess")
      .mockImplementationOnce(() => {});

    require("./main");

    expect(setupChildProcessSpy).toBeCalled();
  });
});
