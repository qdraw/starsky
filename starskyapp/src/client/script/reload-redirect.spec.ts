import * as warmupLocalOrRemote from "./reload-warmup-local-or-remote";

describe("reload redirect", () => {
  it("should call deps", () => {
    const checkSpy = jest
      .spyOn(warmupLocalOrRemote, "warmupLocalOrRemote")
      .mockImplementationOnce(() => {});

    require("./reload-redirect");

    expect(checkSpy).toBeCalled();
  });
});
