import { BrowserWindow } from 'electron';
import * as appConfig from 'electron-settings'

interface IWindowsState {
    x?: number,
    y?: number,
    width: number,
    height: number,
    isMaximized: boolean
}

export async function windowStateKeeper(windowName: string) {

    let window : any;
    let windowState = {} as IWindowsState; 

      async function setBounds() {
        // Restore from appConfig
        if (await appConfig.has(`windowState.${windowName}`)) {
          const result = await appConfig.get(`windowState.${windowName}`);
          windowState = result as unknown as IWindowsState;
          return;
        }

      // Default
      windowState = {
        x: undefined,
        y: undefined,
        width: 1000,
        height: 800,
        isMaximized: false
      };
    }  
  
    function saveState() {
      if (!windowState.isMaximized) {
        windowState = window.getBounds();
      }
      windowState.isMaximized = window.isMaximized();
      console.log(windowState);
      
      appConfig.set(`windowState.${windowName}`, windowState as any);
    }  
  
    function track(win : BrowserWindow) {
      window = win;
      ['resize', 'move', 'close'].forEach(event => {
        win.on(event as any, saveState);
      });
    } 
  
    await setBounds();  
     
     return({
      x: windowState.x,
      y: windowState.y,
      width: windowState.width,
      height: windowState.height,
      isMaximized: windowState.isMaximized,
      track,
    });
  }