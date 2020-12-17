import { IPreloadApi } from "../../preload/IPreloadApi";
import * as warmupLocalOrRemote from "./reload-warmup-local-or-remote";
declare global {
  var api: IPreloadApi;
}

describe("reload redirect", () => {
  it("should call deps", () => {
    (window as any).api = {};
    const checkSpy = jest
      .spyOn(warmupLocalOrRemote, "warmupLocalOrRemote")
      .mockImplementationOnce(() => {});

    // when change also update webpack and html
    require("./reload-redirect");

    expect(checkSpy).toBeCalled();
  });
});
