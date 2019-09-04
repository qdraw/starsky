
export class IsLegacy {
  public IsLegacy = () => {
    return '-ms-scroll-limit' in document.documentElement.style
      && '-ms-ime-align' in document.documentElement.style
      && navigator.userAgent.indexOf("Trident") > -1;
  }
}
export default IsLegacy;

