import { BrowserWindow } from "electron";

export function getNewFocusedWindow(
  x: number,
  y: number
): { x: number; y: number } {
  const currentWindow = BrowserWindow.getFocusedWindow();

  let x1 = x;
  let y1 = y;
  if (currentWindow) {
    const [currentWindowX, currentWindowY] = currentWindow.getPosition();
    x1 = currentWindowX + 10;
    y1 = currentWindowY + 10;
  }
  return { x: x1, y: y1 };
}
