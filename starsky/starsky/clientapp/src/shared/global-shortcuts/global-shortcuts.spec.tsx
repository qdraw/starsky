import * as useHotKeysParent from "../../hooks/use-keyboard/use-hotkeys";
import * as useLocation from "../../hooks/use-location/use-location";
import { GlobalShortcuts } from "./global-shortcuts";

describe("GlobalShortcuts", () => {
  it("should call useHotKeys with the correct arguments", () => {
    const locationObject = {
      location: window.location,
      navigate: jest.fn()
    };

    jest.spyOn(useLocation, "default").mockImplementationOnce(() => locationObject);

    GlobalShortcuts();

    expect(useHotKeysParent).toHaveBeenCalledWith(
      { key: "k", shiftKey: true, ctrlKeyOrMetaKey: true },
      expect.any(Function),
      []
    );

    expect(locationObject.navigate).toHaveBeenCalledWith(expect.any(String));
  });
});
