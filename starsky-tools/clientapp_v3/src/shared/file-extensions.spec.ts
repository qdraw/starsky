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

  describe("GetFileExtensionWithoutDot", () => {
    it("get file ext without extension No Extension", () => {
      var result = fileExt.GetFileExtensionWithoutDot("/test_image");
      expect(result).toBe("")
    });

    it("get file ext without extension com file", () => {
      var result = fileExt.GetFileExtensionWithoutDot("/te_st/test.com");
      expect(result).toBe("com")
    });

    it("get file ext without extension mp4 file", () => {
      var result = fileExt.GetFileExtensionWithoutDot("/te_st/test.mp4");
      expect(result).toBe("mp4")
    });

    it("get file ext without extension Uppercase mp4 file", () => {
      var result = fileExt.GetFileExtensionWithoutDot("/te_st/test.MP4");
      expect(result).toBe("mp4")
    });

    it("get file name without extension space and http file", () => {
      var result = fileExt.GetFileExtensionWithoutDot("http://path/Lists/Test/Attachments/1/Document Test.docx");
      expect(result).toBe("docx")
    });

    it("get file name without extension on root", () => {
      var result = fileExt.GetFileExtensionWithoutDot("/0000000000aaaaa__exifreadingtest00.jpg");
      expect(result).toBe("jpg")
    });
  });

  describe("GetFileNameWithoutExtension", () => {
    it("get file name without extension com file", () => {
      var result = fileExt.GetFileNameWithoutExtension("/te_st/test.com");
      expect(result).toBe("test")
    });

    it("get file name without extension mp4 file", () => {
      var result = fileExt.GetFileNameWithoutExtension("/te_st/test.mp4");
      expect(result).toBe("test")
    });

    it("get file name without extension space and http file", () => {
      var result = fileExt.GetFileNameWithoutExtension("http://path/Lists/Test/Attachments/1/Document Test.docx");
      expect(result).toBe("Document Test")
    });

    it("get file name without extension on root", () => {
      var result = fileExt.GetFileNameWithoutExtension("/0000000000aaaaa__exifreadingtest00.jpg");
      expect(result).toBe("0000000000aaaaa__exifreadingtest00")
    });
  });

});