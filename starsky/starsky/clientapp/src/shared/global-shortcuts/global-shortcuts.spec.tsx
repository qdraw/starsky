import React from "react";
import * as useHotKeysParent from "../../hooks/use-keyboard/use-hotkeys";
import * as useLocation from "../../hooks/use-location/use-location";
import { UrlQuery } from "../url/url-query";
import { GlobalShortcuts } from "./global-shortcuts";

describe("GlobalShortcuts", () => {
  it("command + shift + k", () => {
    const locationObject = {
      location: window.location,
      navigate: jest.fn()
    };

    jest.spyOn(React, "useEffect").mockImplementationOnce((cb) => {
      cb();
    });

    const event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "k",
      metaKey: true,
      shiftKey: true
    });

    jest.spyOn(useHotKeysParent, "default").mockImplementationOnce((_, callback) => {
      if (callback) {
        callback(event);
      }
    });

    const useLocationSpy = jest
      .spyOn(useLocation, "default")
      .mockImplementationOnce(() => locationObject);

    GlobalShortcuts();

    expect(useLocationSpy).toHaveBeenCalled();
    expect(locationObject.navigate).toHaveBeenCalled();
    expect(locationObject.navigate).toHaveBeenCalledWith(new UrlQuery().UrlPreferencesPage());
  });
});
