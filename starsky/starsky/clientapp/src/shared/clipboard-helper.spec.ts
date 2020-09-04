import { createRef } from 'react';
import { ClipboardHelper, IClipboardData } from './clipboard-helper';

describe("ClipboardHelper", () => {
  var clipboardHelper = new ClipboardHelper();
  var clipBoardName = 'starskyClipboardData';

  function refGenerator(text: string): any {
    var div = document.createElement('div');
    div.innerText = text;
    return { current: div } as any;
  }

  function getClipboardData(): IClipboardData {
    var data = sessionStorage.getItem(clipBoardName);
    if (!data) {
      throw new Error('missing session storage data')
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
      var copyResult = clipboardHelper.Copy(refGenerator("1"), refGenerator("2"), refGenerator("3"));
      expect(copyResult).toBeTruthy();

      var clipboard = getClipboardData()

      expect(clipboard.tags).toBe('1');
      expect(clipboard.description).toBe('2');
      expect(clipboard.title).toBe('3');
      deleteClipboard();
    });
  });

  describe("Paste", () => {

    it("non valid ref", () => {
      var ref = createRef<HTMLDivElement>();
      expect(clipboardHelper.Paste(ref, ref, ref)).toBeFalsy();
    });

    it("No clipboard data", () => {
      var copyResult = clipboardHelper.Paste(refGenerator("1"), refGenerator("2"), refGenerator("3"));
      expect(copyResult).toBeFalsy();
    });

    it("Copy and Paste", () => {
      clipboardHelper.Copy(refGenerator("A"), refGenerator("B"), refGenerator("C"));

      var pasteA = refGenerator("1");
      var pasteB = refGenerator("2");
      var pasteC = refGenerator("3");
      clipboardHelper.Paste(pasteA, pasteB, pasteC);

      expect(pasteA.current.innerText).toBe("A");
      expect(pasteB.current.innerText).toBe("B");
      expect(pasteC.current.innerText).toBe("C");
    });

  });

});
