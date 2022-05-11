import { FileExtensions } from "./file-extensions";

describe("FileExtensions", () => {
  var fileExt = new FileExtensions();

  describe("GetParentPath", () => {
    it("get parent path #1", () => {
      var result = fileExt.GetParentPath("/__starsky/test/");
      expect(result).toBe("/__starsky");
    });
    it("get parent path #2", () => {
      var result = fileExt.GetParentPath("/__starsky/test");
      expect(result).toBe("/__starsky");
    });
    it("get parent path #3", () => {
      var result = fileExt.GetParentPath("/__starsky");
      expect(result).toBe("/");
    });
    it("get parent path #4", () => {
      var result = fileExt.GetParentPath("/");
      expect(result).toBe("/");
    });
  });

  describe("GetFileName", () => {
    it("get file name no slash", () => {
      var result = fileExt.GetFileName("testung.jpg");
      expect(result).toBe("testung.jpg");
    });

    it("get file name no ext", () => {
      var result = fileExt.GetFileName("/te_st/test");
      expect(result).toBe("test");
    });

    it("get file name  com file", () => {
      var result = fileExt.GetFileName("/te_st/test.com");
      expect(result).toBe("test.com");
    });

    it("get file name  mp4 file", () => {
      var result = fileExt.GetFileName("/te_st/test.mp4");
      expect(result).toBe("test.mp4");
    });

    it("get file name space and http file", () => {
      var result = fileExt.GetFileName(
        "http://path/Lists/Test/Attachments/1/Document Test.docx"
      );
      expect(result).toBe("Document Test.docx");
    });

    it("get file name on root", () => {
      var result = fileExt.GetFileName(
        "/0000000000aaaaa__exifreadingtest00.jpg"
      );
      expect(result).toBe("0000000000aaaaa__exifreadingtest00.jpg");
    });
  });
  
});
