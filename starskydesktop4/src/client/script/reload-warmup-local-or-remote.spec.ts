/* eslint-disable @typescript-eslint/unbound-method */
import { AppVersionIpcKey } from "../../app/config/app-version-ipc-key.const";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey,
} from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import * as checkForUpdates from "./check-for-updates";
import { warmupLocalOrRemote } from "./reload-warmup-local-or-remote";
import * as warmupScript from "./reload-warmup-script";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
  var api: IPreloadApi;
}

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("reload redirect", () => {
  describe("warmupLocalOrRemote", () => {
    const oldWindowLocation = window.location;
    const assignSpy = jest.fn();

    beforeAll(() => {
      delete window.location;

      (window.location as any) = Object.defineProperties(
        {},
        {
          ...Object.getOwnPropertyDescriptors(oldWindowLocation),
          assign: {
            configurable: true,
            value: (e: string) => {
              return assignSpy(e);
            },
          },
        }
      );
    });

    beforeEach(() => {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access, @typescript-eslint/no-explicit-any
      (window as any).api = {};
    });

    afterEach(() => {
      assignSpy.mockReset();
    });

    afterAll(() => {
      // restore `window.location` to the `jsdom` `Location` object
      window.location = oldWindowLocation;
    });

    it("should get remote ipc key and app version", () => {
      window.api = {
        send: jest.fn(),
        receive: jest.fn(),
      };
      warmupLocalOrRemote();
      expect(window.api.send).toHaveBeenCalled();
      expect(window.api.send).toHaveBeenNthCalledWith(
        1,
        LocationIsRemoteIpcKey,
        null
      );
      expect(window.api.send).toHaveBeenNthCalledWith(
        2,
        AppVersionIpcKey,
        null
      );
    });

    it("should check for updates", () => {
      jest
        .spyOn(warmupScript, "warmupScript")
        .mockImplementationOnce((_, c, m, func) => {
          func(true);
        });

      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      window.api = {
        send: jest.fn(),
        receive: (_, func) => {
          func({ location: "test" });
        },
      };

      const checkForUpdatesSpy = jest
        .spyOn(checkForUpdates, "checkForUpdates")
        .mockImplementationOnce(() => Promise.resolve(200));

      warmupLocalOrRemote();
      expect(window.api.send).toHaveBeenNthCalledWith(
        3,
        LocationUrlIpcKey,
        null
      );

      expect(checkForUpdatesSpy).toHaveBeenCalled();
    });
  });
});
