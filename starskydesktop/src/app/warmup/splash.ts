import { BrowserWindow } from "electron";
import * as path from "path";

export async function SetupSplash(): Promise<BrowserWindow> {
  const splash = new BrowserWindow({
    width: 300,
    height: 150,
    transparent: true,
    frame: false,
    alwaysOnTop: true,
  });

  await splash.loadFile(
    path.join(__dirname, "client", "pages", "splash", "splash.html")
  );

  splash.center();

  splash.show();

  return splash;
}

export function CloseSplash(splash: BrowserWindow): void {
  splash.close();
}
