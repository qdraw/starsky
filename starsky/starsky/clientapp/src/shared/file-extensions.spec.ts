import { FileExtensions } from './file-extensions';

describe("keyboard", () => {
  var fileExt = new FileExtensions();
  describe("MatchExtension", () => {
    it("wrong from input without extension ", () => {
      var result = fileExt.MatchExtension("test", "");
      expect(result).toBeNull();
    });

    it("wrong from input (nothing)", () => {
      var result = fileExt.MatchExtension("", "");
      expect(result).toBeNull();
    });

    it("wrong to output ", () => {
      var result = fileExt.MatchExtension("test.jpg", "");
      expect(result).toBeFalsy();
    });

    it("jpeg != tiff ", () => {
      var result = fileExt.MatchExtension("test.jpg", "test.tiff");
      expect(result).toBeFalsy();
    });
    it("two jpeg files", () => {
      var result = fileExt.MatchExtension("222.jpg", "test1.jpg");
      expect(result).toBeTruthy();
    });
  });

  describe("IsValidFileName", () => {
    it("valid filename", () => {
      var result = fileExt.IsValidFileName("222.jpg");
      expect(result).toBeTruthy();
    });

    it("only extension filename (non valid)", () => {
      var result = fileExt.IsValidFileName(".jpg");
      expect(result).toBeFalsy();
    });

    it("start with underscore _.com", () => {
      var result = fileExt.IsValidFileName("_.com");
      expect(result).toBeTruthy();
    });
  });
});