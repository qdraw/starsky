import { BrowserWindow } from "electron";

export function getNewFocusedWindow(
  x: number,
  y: number
): { x: number; y: number } {
  const currentWindow = BrowserWindow.getFocusedWindow();

  if (currentWindow) {
    const [currentWindowX, currentWindowY] = currentWindow.getPosition();
    x = currentWindowX + 10;
    y = currentWindowY + 10;
  }
  return { x, y };
}
