import * as warmupLocalOrRemote from "./reload-warmup-local-or-remote";

describe("reload redirect", () => {
  it("should call deps", () => {
    const checkSpy = jest
      .spyOn(warmupLocalOrRemote, "warmupLocalOrRemote")
      .mockImplementationOnce(() => {});

    // when change also update webpack and html
    require("./reload-redirect");

    expect(checkSpy).toBeCalled();
  });
});
