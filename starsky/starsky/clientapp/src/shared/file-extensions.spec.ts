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

  describe("IsValidDirectoryName", () => {
    it("valid DirectoryName", () => {
      var result = fileExt.IsValidDirectoryName("222");
      expect(result).toBeTruthy();
    });

    it("non valid directory name to short", () => {
      var result = fileExt.IsValidDirectoryName("d");
      expect(result).toBeFalsy();
    });

    it("non valid directory name string.emthy", () => {
      var result = fileExt.IsValidDirectoryName("");
      expect(result).toBeFalsy();
    });

    it("non valid directory name", () => {
      var result = fileExt.IsValidDirectoryName(".èjpg");
      expect(result).toBeFalsy();
    });

    it("start with underscore _test", () => {
      var result = fileExt.IsValidDirectoryName("_test");
      expect(result).toBeTruthy();
    });
  });

  describe("GetParentPath", () => {
    it("get parent path #1", () => {
      var result = fileExt.GetParentPath("/__starsky/test/");
      expect(result).toBe("/__starsky")
    });
    it("get parent path #2", () => {
      var result = fileExt.GetParentPath("/__starsky/test");
      expect(result).toBe("/__starsky")
    });
    it("get parent path #3", () => {
      var result = fileExt.GetParentPath("/__starsky");
      expect(result).toBe("/")
    });
    it("get parent path #4", () => {
      var result = fileExt.GetParentPath("/");
      expect(result).toBe("/")
    });
  });
});