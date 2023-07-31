export class BrowserDetect {
  IsIOS() {
    const isWebKit =
      "WebkitAppearance" in document.documentElement.style &&
      navigator.userAgent.indexOf("Safari") !== -1;
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);

    const isIPadOS = "ontouchend" in document && navigator.maxTouchPoints > 1;
    return isWebKit && (isIOS || isIPadOS);
  }
  public IsLegacy = (): boolean => {
    if (!("fetch" in window)) return true;
    return (
      "-ms-scroll-limit" in document.documentElement.style &&
      "-ms-ime-align" in document.documentElement.style &&
      navigator.userAgent.indexOf("Trident") > -1
    );
  };

  public IsElectronApp = (): boolean => {
    if ((window as any).isElectron === true) return true;
    return (
      navigator.userAgent.indexOf("Electron") > -1 &&
      navigator.userAgent.indexOf("starsky/") > -1
    );
  };
}
export default BrowserDetect;
