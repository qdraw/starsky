import { BrowserDetect } from "./browser-detect";
import DetectAutomaticRotation, { testAutoOrientationImageURL } from "./detect-automatic-rotation";

class ImageClass {
  constructor() {
    setTimeout(() => {
      (this as unknown as { onload: () => void }).onload(); // simulate success
    }, 1);
  }
}

describe("select", () => {
  describe("removeSidebarSelection", () => {
    jest.mock("../style/images/detect-automatic-rotation.jpg");

    it("returns true if both arguments are null or undefined", async () => {
      // Fake onLoad Trigger
      // @see: https://stackoverflow.com/a/59118461

      (global as unknown as { Image: typeof ImageClass }).Image = ImageClass;

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
