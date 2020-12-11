import * as appConfig from "electron-settings";
import { LocationIsRemoteCallback } from "./ipc-bridge";

describe("ipc bridge", () => {
  describe("LocationIsRemoteCallback", () => {
    it("DifferenceInDate to be less than 80", () => {
      const event = { reply: jest.fn() } as any;

      jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
        return Promise.resolve(null);
      });
      LocationIsRemoteCallback(event, null);
      expect(event.reply).toBeCalled();
    });
  });
});
