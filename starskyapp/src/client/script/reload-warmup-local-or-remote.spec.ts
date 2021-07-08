import { AppVersionIpcKey } from "../../app/config/app-version-ipc-key.const";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import * as checkForUpdates from "./check-for-updates";
import { warmupLocalOrRemote } from "./reload-warmup-local-or-remote";
import * as warmupScript from "./reload-warmup-script";
declare global {
  var api: IPreloadApi;
}

describe("reload redirect", () => {
  describe("warmupLocalOrRemote", () => {
    const oldWindowLocation = window.location;
    const assignSpy = jest.fn();
    let assignLocation = "";

    beforeAll(() => {
      delete window.location;

      (window.location as any) = Object.defineProperties(
        {},
        {
          ...Object.getOwnPropertyDescriptors(oldWindowLocation),
          assign: {
            configurable: true,
            value: (e: string) => {
              assignLocation = e;
              return assignSpy(e);
            }
          }
        }
      );
    });

    beforeEach(() => {
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
        receive: jest.fn()
      };
      warmupLocalOrRemote();
      expect(window.api.send).toBeCalled();
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
          return;
        });

      // @ts-ignore
      window.api = {
        send: jest.fn(),
        receive: (_, func) => {
          func({ location: "test" });
        }
      };

      const checkForUpdatesSpy = jest
        .spyOn(checkForUpdates, "checkForUpdates")
        .mockImplementationOnce(() => Promise.resolve());

      warmupLocalOrRemote();
      expect(window.api.send).toHaveBeenNthCalledWith(
        3,
        LocationUrlIpcKey,
        null
      );

      expect(checkForUpdatesSpy).toBeCalled();
    });
  });
});
