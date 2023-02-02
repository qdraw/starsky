/* eslint-disable @typescript-eslint/no-floating-promises */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";

interface IWindowsState {
  x?: number;
  y?: number;
  width: number;
  height: number;
  isMaximized: boolean;
  track?: (win: BrowserWindow) => void;
}

export async function windowStateKeeper(
  windowName: string
): Promise<IWindowsState> {
  let window: any;
  let windowState = {} as IWindowsState;

  async function setBounds() {
    // Restore from appConfig
    if (await appConfig.has(`windowState.${windowName}`)) {
      const result = await appConfig.get(`windowState.${windowName}`);
      windowState = result as unknown as IWindowsState;
      if (!Number.isNaN(windowState.x) && !Number.isNaN(windowState.y)) {
        return;
      }
    }

    // Default
    windowState = {
      x: undefined,
      y: undefined,
      width: 1000,
      height: 800,
      isMaximized: false,
    };
  }

  function saveState() {
    if (!windowState.isMaximized) {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
      windowState = window.getBounds() as IWindowsState;
    }
    windowState.isMaximized = window.isMaximized() as boolean;

    if (windowState.width <= 40) {
      windowState.width = 20;
    }
    if (windowState.height <= 40) {
      windowState.height = 40;
    }

    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    appConfig.set(`windowState.${windowName}`, windowState as any);
  }

  function track(win: BrowserWindow) {
    window = win;
    ["resize", "move", "close"].forEach((event: string) => {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      win.on(event as any, saveState);
    });
  }

  await setBounds();

  return {
    x: windowState.x,
    y: windowState.y,
    width: windowState.width,
    height: windowState.height,
    isMaximized: windowState.isMaximized,
    track,
  };
}
