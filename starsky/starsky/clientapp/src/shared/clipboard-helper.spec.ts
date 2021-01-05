import { createRef } from "react";
import { ClipboardHelper, IClipboardData } from "./clipboard-helper";

describe("ClipboardHelper", () => {
  var clipboardHelper = new ClipboardHelper();
  var clipBoardName = "starskyClipboardData";

  function refGenerator(text: string): any {
    var div = document.createElement("div");
    div.innerText = text;
    return { current: div } as any;
  }

  function getClipboardData(): IClipboardData {
    var data = sessionStorage.getItem(clipBoardName);
    if (!data) {
      throw new Error("missing session storage data");
    }
    return JSON.parse(data);
  }

  function deleteClipboard() {
    sessionStorage.removeItem(clipBoardName);
  }

  describe("Copy", () => {
    it("non valid ref", () => {
      var ref = createRef<HTMLDivElement>();
      expect(clipboardHelper.Copy(ref, ref, ref)).toBeFalsy();
    });

    it("Copy data", () => {
      var copyResult = clipboardHelper.Copy(
        refGenerator("1"),
        refGenerator("2"),
        refGenerator("3")
      );
      expect(copyResult).toBeTruthy();

      var clipboard = getClipboardData();

      expect(clipboard.tags).toBe("1");
      expect(clipboard.description).toBe("2");
      expect(clipboard.title).toBe("3");
      deleteClipboard();
    });
  });

  describe("Paste", () => {
    it("non valid ref", () => {
      expect(clipboardHelper.Paste(null as any)).toBeFalsy();
    });

    it("No clipboard data", () => {
      const callback = jest.fn();
      var copyResult = clipboardHelper.Paste(callback);
      expect(copyResult).toBeFalsy();
    });

    it("Copy and Paste", () => {
      const callback = jest.fn();

      clipboardHelper.Copy(
        refGenerator("A"),
        refGenerator("B"),
        refGenerator("C")
      );

      const result = clipboardHelper.Paste(callback);

      expect(result).toBeTruthy();

      expect(callback).toBeCalled();

      expect(callback).toHaveBeenNthCalledWith(1, "A", "tags");
      expect(callback).toHaveBeenNthCalledWith(2, "B", "description");
      expect(callback).toHaveBeenNthCalledWith(3, "C", "title");
    });
  });
});
