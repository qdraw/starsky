import { OnLoadMouseAction } from "./on-load-mouse-action";

describe("OnLoadMouseAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should also set when SetError and SetIsLoading is null", () => {
    const setImage = jest.fn();
    new OnLoadMouseAction(setImage, null as any, null as any).onLoad({
      target: {}
    } as any);
    expect(setImage).toBeCalled();
  });
});
