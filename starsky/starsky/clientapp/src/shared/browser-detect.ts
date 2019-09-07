
export class BrowserDetect {
  IsIOS() {
    return 'WebkitAppearance' in document.documentElement.style && navigator.userAgent.indexOf("Safari") !== -1 &&
      (navigator.userAgent.indexOf("iPad") !== -1 || navigator.userAgent.indexOf("iPhone") !== -1)
  }
  public IsLegacy = () => {
    return '-ms-scroll-limit' in document.documentElement.style
      && '-ms-ime-align' in document.documentElement.style
      && navigator.userAgent.indexOf("Trident") > -1;
  }
}
export default BrowserDetect;

