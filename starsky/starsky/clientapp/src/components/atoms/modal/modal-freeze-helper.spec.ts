import * as capturePosition from "../../../hooks/use-capture-position";
import modalFreezeHelper, { toggleTabIndex } from "./modal-freeze-helper";

describe("toggleTabIndex", () => {
  it("test set tabIndex", () => {
    const div = document.createElement("div");
    const aHref = document.createElement("a");
    div.appendChild(aHref);

    toggleTabIndex("off", div);
    expect(aHref.tabIndex).toBe(-1);
  });

  it("test remove tabIndex", () => {
    const div = document.createElement("div");
    const aHref = document.createElement("a");
    div.appendChild(aHref);

    toggleTabIndex("off", div);
    expect(aHref.tabIndex).toBe(-1);

    toggleTabIndex("on", div);

    expect(aHref.tabIndex).toBe(0);
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
      jest.fn() as any,
      "root",
      "id",
      true,
      { current: null },
      div
    );
    expect(freezeSpy).toBeCalled();
    expect(unfreezeSpy).not.toBeCalled();
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
      jest.fn() as any,
      "root",
      "id",
      false,
      { current: null },
      div
    );
    expect(freezeSpy).not.toBeCalled();
    expect(unfreezeSpy).toBeCalled();
  });
});
