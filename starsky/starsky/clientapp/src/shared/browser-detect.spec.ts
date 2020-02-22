import BrowserDetect from './browser-detect';

describe("browser-detect", () => {
  it("is not ios", () => {
    expect(new BrowserDetect().IsIOS()).toBeFalsy()
  });

  it("is not IsLegacy", () => {
    expect(new BrowserDetect().IsLegacy()).toBeFalsy()
  });

  it("Fake IE11", () => {
    (document.documentElement.style as any)['-ms-scroll-limit'] = true;
    (document.documentElement.style as any)['-ms-ime-align'] = true;

    jest.spyOn(navigator, 'userAgent', 'get').mockImplementationOnce(() => {
      return "Trident";
    });
    expect(new BrowserDetect().IsLegacy()).toBeTruthy()
  });

  it("Fake IsIOS iphone", () => {
    (document.documentElement.style as any)['WebkitAppearance'] = true;
    jest.spyOn(navigator, 'userAgent', 'get').mockImplementationOnce(() => {
      return "Safari";
    });
    jest.spyOn(navigator, 'platform', 'get').mockImplementationOnce(() => {
      return "iPhone";
    });
    expect(new BrowserDetect().IsIOS()).toBeTruthy()
  });

});