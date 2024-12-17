import { SyntheticEvent } from "react";
import { OnLoadMouseAction } from "./on-load-mouse-action";

describe("OnLoadMouseAction", () => {
  // @see: pan-and-zoom-image.spec.tsx

  it("should also set when SetError and SetIsLoading is null", () => {
    const setImage = jest.fn();
    new OnLoadMouseAction(
      setImage,
      null as unknown as React.Dispatch<React.SetStateAction<boolean>>,
      null as unknown as React.Dispatch<React.SetStateAction<boolean>>
    ).onLoad({
      target: {}
    } as unknown as SyntheticEvent<HTMLImageElement, Event>);
    expect(setImage).toHaveBeenCalled();
  });
});
