export class BrowserDetect {
  IsIOS() {
    const isWebKit =
      "WebkitAppearance" in document.documentElement.style &&
      navigator.userAgent.includes("Safari");
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);

    const isIPadOS = "ontouchend" in document && navigator.maxTouchPoints > 1;
    return isWebKit && (isIOS || isIPadOS);
  }
  public IsLegacy = (): boolean => {
    if (!("fetch" in globalThis)) return true;
    return (
      "-ms-scroll-limit" in document.documentElement.style &&
      "-ms-ime-align" in document.documentElement.style &&
      navigator.userAgent.includes("Trident")
    );
  };

  public IsElectronApp = (): boolean => {
    if ((globalThis as { isElectron?: boolean }).isElectron === true) return true;
    return navigator.userAgent.includes("Electron") && navigator.userAgent.includes("starsky/");
  };
}
