import { BrowserDetect } from "./browser-detect";

describe("browser-detect", () => {
  it("is not ios", () => {
    expect(new BrowserDetect().IsIOS()).toBeFalsy();
  });

  it("is not IsLegacy", () => {
    expect(new BrowserDetect().IsLegacy()).toBeFalsy();
  });

  it("Fake IE11", () => {
    const style = document.documentElement.style as unknown as { [key: string]: string };
    style["-ms-scroll-limit"] = "true";
    style["-ms-ime-align"] = "true";

    jest.spyOn(navigator, "userAgent", "get").mockImplementationOnce(() => {
      return "Trident";
    });
    expect(new BrowserDetect().IsLegacy()).toBeTruthy();
  });

  it("Fake IsIOS iphone", () => {
    const style = document.documentElement.style as unknown as { [key: string]: string };
    style["WebkitAppearance"] = "true";
    jest.spyOn(navigator, "userAgent", "get").mockImplementationOnce(() => {
      return "Safari";
    });
    jest.spyOn(navigator, "userAgent", "get").mockImplementationOnce(() => {
      return "iPhone";
    });
    expect(new BrowserDetect().IsIOS()).toBeTruthy();
  });

  it("Fake IsIOS iPad", () => {
    const style = document.documentElement.style as unknown as { [key: string]: string };
    style["WebkitAppearance"] = "true";

    document.ontouchend = jest.fn();

    Object.defineProperty(navigator, "maxTouchPoints", {
      value: 2
    });
    jest.spyOn(navigator, "userAgent", "get").mockImplementationOnce(() => {
      return "Safari";
    });

    expect(new BrowserDetect().IsIOS()).toBeTruthy();
  });
});
