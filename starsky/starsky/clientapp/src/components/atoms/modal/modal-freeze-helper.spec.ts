import * as capturePosition from "../../../hooks/use-capture-position";
import modalFreezeHelper, {
  modalFreezeOpen,
  modalUnFreezeNotOpen
} from "./modal-freeze-helper";
import * as toggleTabIndex from "./toggle-tab-index";

describe("modalFreezeOpen", () => {
  it("modalFreezeOpen", () => {
    const freeze = jest.fn();
    modalFreezeOpen(freeze, { current: null } as any, null, null);
    expect(freeze).toBeCalled();
  });
});

describe("modalUnFreezeNotOpen", () => {
  it("modalUnFreezeNotOpen null so not called", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest
      .spyOn(toggleTabIndex, "toggleTabIndex")
      .mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, null, null, undefined, {
      current: false
    });
    expect(unFreeze).toBeCalled();
    expect(toggleTabSpy).not.toBeCalled();
  });

  it("modalUnFreezeNotOpen modalContainer off", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest
      .spyOn(toggleTabIndex, "toggleTabIndex")
      .mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, { current: true } as any, null, undefined, {
      current: false
    });
    expect(unFreeze).toBeCalled();
    expect(toggleTabSpy).toBeCalled();
    expect(toggleTabSpy).toBeCalledWith("off", { current: true });
  });

  it("modalUnFreezeNotOpen modalContainer", () => {
    const unFreeze = jest.fn();
    const toggleTabSpy = jest
      .spyOn(toggleTabIndex, "toggleTabIndex")
      .mockImplementation(() => {});

    modalUnFreezeNotOpen(unFreeze, null, { current: true } as any, undefined, {
      current: false
    });
    expect(unFreeze).toBeCalled();
    expect(toggleTabSpy).toBeCalled();
    expect(toggleTabSpy).toBeCalledWith("on", { current: true });
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
