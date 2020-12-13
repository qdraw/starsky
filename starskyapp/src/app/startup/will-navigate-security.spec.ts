import * as appConfig from "electron-settings";
import { willNavigateSecurity } from "./will-navigate-security";

describe("willNavigateSecurity", () => {
  it("stop random navigate", async () => {
    jest
      .spyOn(appConfig, "get")
      .mockImplementationOnce(() => Promise.resolve(null));

    const preventDefaultSpy = jest.fn();
    await willNavigateSecurity(
      { preventDefault: preventDefaultSpy } as any,
      "http://google.com"
    );
    expect(preventDefaultSpy).toBeCalled();
    expect(preventDefaultSpy).toBeCalledTimes(1);
  });

  it("allow localhost", async () => {
    jest
      .spyOn(appConfig, "get")
      .mockImplementationOnce(() => Promise.resolve(null));

    const preventDefaultSpy = jest.fn();
    await willNavigateSecurity(
      { preventDefault: preventDefaultSpy } as any,
      "http://localhost:5000"
    );
    expect(preventDefaultSpy).toBeCalledTimes(0);
  });

  it("allow file://", async () => {
    jest
      .spyOn(appConfig, "get")
      .mockImplementationOnce(() => Promise.resolve(null));

    const preventDefaultSpy = jest.fn();
    await willNavigateSecurity(
      { preventDefault: preventDefaultSpy } as any,
      "file://t"
    );
    expect(preventDefaultSpy).toBeCalledTimes(0);

    jest.spyOn(appConfig, "get").mockReset();
  });

  it("allow input from settting", async () => {
    jest
      .spyOn(appConfig, "get")
      .mockImplementationOnce(() => Promise.resolve("http://test.com"));

    const preventDefaultSpy = jest.fn();
    await willNavigateSecurity(
      { preventDefault: preventDefaultSpy } as any,
      "http://test.com"
    );
    expect(preventDefaultSpy).toBeCalledTimes(0);
  });
});
