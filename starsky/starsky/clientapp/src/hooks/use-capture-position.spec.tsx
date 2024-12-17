import { mountReactHook } from "./___tests___/test-hook";
import capturePosition, { ICaptionPosition } from "./use-capture-position";

describe("capturePosition", () => {
  let setupComponent;
  let hook: ICaptionPosition;
  let scrollToSpy: jest.SpyInstance<void, [number, number]>;

  beforeEach(() => {
    setupComponent = mountReactHook(capturePosition, []); // Mount a Component with our hook
    hook = setupComponent.componentHook as ICaptionPosition;

    scrollToSpy = jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("freeze", () => {
    hook.freeze();

    expect(document.body.style.top).toBe("0px");
    expect(document.body.style.position).toBe("fixed");
    expect(scrollToSpy).toHaveBeenCalledTimes(0);

    scrollToSpy.mockReset();
  });

  it("unfreeze", () => {
    hook.unfreeze();

    expect(scrollToSpy).toHaveBeenCalledTimes(1);
  });
});
