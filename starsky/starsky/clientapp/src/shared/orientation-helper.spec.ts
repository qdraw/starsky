import { OrientationHelper } from './orientation-helper';

describe("select", () => {
  describe("removeSidebarSelection", () => {
    jest.mock('../style/images/detect-automatic-rotation.jpg')

    it("returns true if both arguments are null or undefined", async () => {
      // Fake onLoad Trigger
      // @see: https://stackoverflow.com/a/59118461
      (global as any).Image = class {
        constructor() {
          setTimeout(() => {
            (this as any).onload(); // simulate success
          }, 1);
        }
      }
      expect(new OrientationHelper().testAutoOrientationImageURL).toContain('data:image');

      var result = await new OrientationHelper().DetectAutomaticRotation();
      expect(result).toBeFalsy();
    });
  });
});