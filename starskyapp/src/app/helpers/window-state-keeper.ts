import * as appConfig from 'electron-settings'

interface IWindowsState {
    x?: number,
    y?: number,
    width: number,
    height: number,
    isMaximized: boolean
}

export function windowStateKeeper(windowName: string) {

    let window : any;
    let windowState = {} as IWindowsState; 

      function setBounds() {
      // Restore from appConfig
      if (appConfig.has(`windowState.${windowName}`)) {
        windowState = appConfig.get(`windowState.${windowName}`) as any;
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
      appConfig.set(`windowState.${windowName}`, windowState as any);
    }  
  
    function track(win : any) {
      window = win;
      ['resize', 'move', 'close'].forEach(event => {
        win.on(event, saveState);
      });
    } 
  
    setBounds();  
     
     return({
      x: windowState.x,
      y: windowState.y,
      width: windowState.width,
      height: windowState.height,
      isMaximized: windowState.isMaximized,
      track,
    });
  }