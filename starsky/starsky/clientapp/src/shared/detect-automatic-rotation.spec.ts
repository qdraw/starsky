import { BrowserDetect } from "./browser-detect";
import DetectAutomaticRotation, { testAutoOrientationImageURL } from "./detect-automatic-rotation";

describe("select", () => {
  describe("removeSidebarSelection", () => {
    jest.mock("../style/images/detect-automatic-rotation.jpg");

    it("returns true if both arguments are null or undefined", async () => {
      // Fake onLoad Trigger
      // @see: https://stackoverflow.com/a/59118461
      (global as any).Image = class {
        constructor() {
          setTimeout(() => {
            (this as any).onload(); // simulate success
          }, 1);
        }
      };
      expect(testAutoOrientationImageURL).toContain("data:image");

      const result = await DetectAutomaticRotation();
      expect(result).toBeFalsy();
    });

    it("iOS should be true", async () => {
      jest.spyOn(BrowserDetect.prototype, "IsIOS").mockImplementationOnce(() => {
        return true;
      });

      const result = await DetectAutomaticRotation();
      expect(result).toBeTruthy();
    });
  });
});
