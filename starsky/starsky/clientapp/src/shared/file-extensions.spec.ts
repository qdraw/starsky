import { FileExtensions } from './file-extensions';

describe("keyboard", () => {
  var fileExt = new FileExtensions();
  describe("MatchExtension", () => {
    it("null input", () => {
      var result = fileExt.MatchExtension("test", "");

      expect(result).toBeNull();
    });
  });
});