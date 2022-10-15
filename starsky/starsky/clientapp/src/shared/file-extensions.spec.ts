import { FileExtensions } from "./file-extensions";

describe("keyboard", () => {
  const fileExt = new FileExtensions();
  describe("MatchExtension", () => {
    it("wrong from input without extension", () => {
      const result = fileExt.MatchExtension("test", "");
      expect(result).toBeNull();
    });

    it("wrong from input (nothing)", () => {
      const result = fileExt.MatchExtension("", "");
      expect(result).toBeNull();
    });

    it("wrong to output", () => {
      const result = fileExt.MatchExtension("test.jpg", "");
      expect(result).toBeFalsy();
    });

    it("jpeg != tiff", () => {
      const result = fileExt.MatchExtension("test.jpg", "test.tiff");
      expect(result).toBeFalsy();
    });
    it("two jpeg files", () => {
      const result = fileExt.MatchExtension("222.jpg", "test1.jpg");
      expect(result).toBeTruthy();
    });
  });

  describe("IsValidFileName", () => {
    it("valid filename", () => {
      const result = fileExt.IsValidFileName("222.jpg");
      expect(result).toBeTruthy();
    });

    it("only extension filename (non valid)", () => {
      const result = fileExt.IsValidFileName(".jpg");
      expect(result).toBeFalsy();
    });

    it("start with underscore _.com", () => {
      const result = fileExt.IsValidFileName("_.com");
      expect(result).toBeTruthy();
    });
  });

  describe("IsValidDirectoryName", () => {
    it("valid DirectoryName", () => {
      const result = fileExt.IsValidDirectoryName("222");
      expect(result).toBeTruthy();
    });

    it("non valid directory name to short", () => {
      const result = fileExt.IsValidDirectoryName("d");
      expect(result).toBeFalsy();
    });

    it("non valid directory name string.emthy", () => {
      const result = fileExt.IsValidDirectoryName("");
      expect(result).toBeFalsy();
    });

    it("non valid directory name", () => {
      const result = fileExt.IsValidDirectoryName(".èjpg");
      expect(result).toBeFalsy();
    });

    it("start with underscore _test", () => {
      const result = fileExt.IsValidDirectoryName("_test");
      expect(result).toBeTruthy();
    });
  });

  describe("GetParentPath", () => {
    it("get parent path #1", () => {
      const result = fileExt.GetParentPath("/__starsky/test/");
      expect(result).toBe("/__starsky");
    });
    it("get parent path #2", () => {
      const result = fileExt.GetParentPath("/__starsky/test");
      expect(result).toBe("/__starsky");
    });
    it("get parent path #3", () => {
      const result = fileExt.GetParentPath("/__starsky");
      expect(result).toBe("/");
    });
    it("get parent path #4", () => {
      const result = fileExt.GetParentPath("/");
      expect(result).toBe("/");
    });
  });

  describe("GetFileExtensionWithoutDot", () => {
    it("get file ext without extension No Extension", () => {
      const result = fileExt.GetFileExtensionWithoutDot("/test_image");
      expect(result).toBe("");
    });

    it("get file ext without extension com file", () => {
      const result = fileExt.GetFileExtensionWithoutDot("/te_st/test.com");
      expect(result).toBe("com");
    });

    it("get file ext without extension mp4 file", () => {
      const result = fileExt.GetFileExtensionWithoutDot("/te_st/test.mp4");
      expect(result).toBe("mp4");
    });

    it("get file ext without extension Uppercase mp4 file", () => {
      const result = fileExt.GetFileExtensionWithoutDot("/te_st/test.MP4");
      expect(result).toBe("mp4");
    });

    it("get file name without extension space and http file", () => {
      const result = fileExt.GetFileExtensionWithoutDot(
        "http://path/Lists/Test/Attachments/1/Document Test.docx"
      );
      expect(result).toBe("docx");
    });

    it("get file name without extension on root", () => {
      const result = fileExt.GetFileExtensionWithoutDot(
        "/0000000000aaaaa__exifreadingtest00.jpg"
      );
      expect(result).toBe("jpg");
    });
  });

  describe("GetFileName", () => {
    it("get file name no slash", () => {
      const result = fileExt.GetFileName("testung.jpg");
      expect(result).toBe("testung.jpg");
    });

    it("get file name no ext", () => {
      const result = fileExt.GetFileName("/te_st/test");
      expect(result).toBe("test");
    });

    it("get file name  com file", () => {
      const result = fileExt.GetFileName("/te_st/test.com");
      expect(result).toBe("test.com");
    });

    it("get file name  mp4 file", () => {
      const result = fileExt.GetFileName("/te_st/test.mp4");
      expect(result).toBe("test.mp4");
    });

    it("get file name space and http file", () => {
      const result = fileExt.GetFileName(
        "http://path/Lists/Test/Attachments/1/Document Test.docx"
      );
      expect(result).toBe("Document Test.docx");
    });

    it("get file name on root", () => {
      const result = fileExt.GetFileName(
        "/0000000000aaaaa__exifreadingtest00.jpg"
      );
      expect(result).toBe("0000000000aaaaa__exifreadingtest00.jpg");
    });
  });

  describe("GetFileNameWithoutExtension", () => {
    it("get file name without extension com file", () => {
      const result = fileExt.GetFileNameWithoutExtension("/te_st/test.com");
      expect(result).toBe("test");
    });

    it("get file name without extension mp4 file", () => {
      const result = fileExt.GetFileNameWithoutExtension("/te_st/test.mp4");
      expect(result).toBe("test");
    });

    it("get file name without extension space and http file", () => {
      const result = fileExt.GetFileNameWithoutExtension(
        "http://path/Lists/Test/Attachments/1/Document Test.docx"
      );
      expect(result).toBe("Document Test");
    });

    it("get file name without extension on root", () => {
      const result = fileExt.GetFileNameWithoutExtension(
        "/0000000000aaaaa__exifreadingtest00.jpg"
      );
      expect(result).toBe("0000000000aaaaa__exifreadingtest00");
    });
  });
});
