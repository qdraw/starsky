import { IPreloadApi } from "../../preload/IPreloadApi";
import * as warmupLocalOrRemote from "./reload-warmup-local-or-remote";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
  var api: IPreloadApi;
}

describe("reload redirect", () => {
  it("should call deps", () => {
    window.api = {
      receive: jest.fn(),
      send: jest.fn()
    };
    const checkSpy = jest
      .spyOn(warmupLocalOrRemote, "warmupLocalOrRemote")
      .mockImplementationOnce(() => {});

    // when change also update webpack and html
    // eslint-disable-next-line global-require
    require("./reload-redirect");

    expect(checkSpy).toHaveBeenCalled();
  });
});
