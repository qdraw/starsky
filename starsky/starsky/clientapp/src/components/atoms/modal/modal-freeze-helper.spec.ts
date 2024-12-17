import * as capturePosition from "../../../hooks/use-capture-position";
import modalFreezeHelper, { modalFreezeOpen, modalUnFreezeNotOpen } from "./modal-freeze-helper";
import * as toggleTabIndex from "./toggle-tab-index";

describe("modalFreezeOpen", () => {
  it("modalFreezeOpen", () => {
    const freeze = jest.fn();
    modalFreezeOpen(freeze, { current: null } as React.RefObject<HTMLButtonElement>, null, null);
    expect(freeze).toHaveBeenCalled();
  });

  it("modalFreezeOpen focus", () => {
    const focus = jest.fn();
    modalFreezeOpen(
      jest.fn(),
      { current: { focus } } as unknown as React.RefObject<HTMLButtonElement>,
      null,
      null
    );
    expect(focus).toHaveBeenCalled();
  });

  it("modalFreezeOpen rootContainer", () => {
    const freeze = jest.fn();
    modalFreezeOpen(
      freeze,
      { current: null } as React.RefObject<HTMLButtonElement>,
      null,
      document.createElement("div") as HTMLElement
    );
    expect(freeze).toHaveBeenCalled();
  });
});

describe("modalUnFreezeNotOpen", () => {
  it("modalUnFreezeNotOpen null so not called", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest.spyOn(toggleTabIndex, "toggleTabIndex").mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, null, null, undefined, {
      current: false
    });
    expect(unFreeze).toHaveBeenCalled();
    expect(toggleTabSpy).not.toHaveBeenCalled();
  });

  it("modalUnFreezeNotOpen modalContainer off", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest.spyOn(toggleTabIndex, "toggleTabIndex").mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, { current: true } as unknown as Element, null, undefined, {
      current: false
    });
    expect(unFreeze).toHaveBeenCalled();
    expect(toggleTabSpy).toHaveBeenCalled();
    expect(toggleTabSpy).toHaveBeenCalledWith("off", { current: true });
  });

  it("modalUnFreezeNotOpen modalContainer", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest.spyOn(toggleTabIndex, "toggleTabIndex").mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, null, { current: true } as unknown as Element, undefined, {
      current: false
    });
    expect(unFreeze).toHaveBeenCalled();
    expect(toggleTabSpy).toHaveBeenCalled();
    expect(toggleTabSpy).toHaveBeenCalledWith("on", { current: true });
  });
});

describe("modalFreezeHelper", () => {
  it("should set freeze", () => {
    const div = document.createElement("div");

    const freezeSpy = jest.fn();
    const unfreezeSpy = jest.fn();

    jest.spyOn(capturePosition, "default").mockImplementation(() => ({
      freeze: freezeSpy,
      unfreeze: unfreezeSpy
    }));

    modalFreezeHelper(
      jest.fn() as unknown as React.MutableRefObject<boolean>,
      "root",
      "id",
      true,
      { current: null },
      div
    );
    expect(freezeSpy).toHaveBeenCalled();
    expect(unfreezeSpy).not.toHaveBeenCalled();
  });

  it("should set unfreeze", () => {
    const div = document.createElement("div");

    const freezeSpy = jest.fn();
    const unfreezeSpy = jest.fn();

    jest.spyOn(capturePosition, "default").mockImplementation(() => ({
      freeze: freezeSpy,
      unfreeze: unfreezeSpy
    }));

    modalFreezeHelper(
      jest.fn() as unknown as React.MutableRefObject<boolean>,
      "root",
      "id",
      false,
      { current: null },
      div
    );
    expect(freezeSpy).not.toHaveBeenCalled();
    expect(unfreezeSpy).toHaveBeenCalled();
  });
});
