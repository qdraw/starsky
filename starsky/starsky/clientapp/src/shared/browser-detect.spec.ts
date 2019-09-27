import BrowserDetect from './browser-detect';

describe("browser-detect", () => {
  it("is not ios", () => {
    expect(new BrowserDetect().IsIOS()).toBeFalsy()
  });
  it("is not IsLegacy", () => {
    expect(new BrowserDetect().IsLegacy()).toBeFalsy()
  });
});