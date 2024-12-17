import { Keyboard } from "./keyboard";

describe("keyboard", () => {
  const keyboard = new Keyboard();
  describe("isInForm", () => {
    it("null input", () => {
      // new EventTarget() = not supported in Safari

      const result = keyboard.isInForm(undefined);

      expect(result).toBeNull();
    });

    it("null input 2", () => {
      // new EventTarget() = not supported in Safari
      const eventTarget: EventTarget = new EventTarget();
      const event = new KeyboardEvent("keydown", {
        keyCode: 37,
        target: eventTarget
      } as KeyboardEventInit);

      const result = keyboard.isInForm(event);

      expect(result).toBeNull();
    });

    it("should ignore form-control", () => {
      const target = document.createElement("div");
      target.className = "form-control";

      const event = new KeyboardEvent("keydown", {
        keyCode: 37
      } as KeyboardEventInit);
      Object.defineProperty(event, "target", {
        writable: false,
        value: target
      });

      const result = keyboard.isInForm(event);

      expect(result).toBeTruthy();
    });

    it("should ignore modal", () => {
      const target = document.createElement("div");
      target.className = "modal";

      const event = new KeyboardEvent("keydown", {
        keyCode: 37
      } as KeyboardEventInit);
      Object.defineProperty(event, "target", {
        writable: false,
        value: target
      });

      const result = keyboard.isInForm(event);

      expect(result).toBeTruthy();
    });
  });

  describe("SetFocusOnEndField", () => {
    it("no child items", () => {
      const focusSpy = jest.fn();
      new Keyboard().SetFocusOnEndField({
        focus: focusSpy,
        childNodes: []
      } as unknown as HTMLDivElement);

      expect(focusSpy).toHaveBeenCalledTimes(1);
    });

    it("text content", () => {
      const focusSpy = jest.fn();
      new Keyboard().SetFocusOnEndField({
        focus: focusSpy,
        childNodes: [{}] // text content is missing
      } as unknown as HTMLDivElement);

      expect(focusSpy).toHaveBeenCalledTimes(0);
    });

    it("missing range", () => {
      const focusSpy = jest.fn();
      const selectionSpy = jest.spyOn(window, "getSelection").mockImplementationOnce(() => null);
      jest.spyOn(document, "createRange").mockImplementationOnce(() => {
        return {
          setStart: jest.fn(),
          collapse: jest.fn()
        } as unknown as Range;
      });
      new Keyboard().SetFocusOnEndField({
        focus: focusSpy,
        childNodes: [
          {
            textContent: "hi"
          }
        ]
      } as unknown as HTMLDivElement);

      expect(selectionSpy).toHaveBeenCalledTimes(1);
    });

    it("input", () => {
      const target = document.createElement("div");
      target.className = "test";
      target.innerHTML = "input";

      const setStart = jest.fn();
      (document as { createRange: () => void }).createRange = () => ({
        setStart: () => setStart,
        setEnd: () => {},
        commonAncestorContainer: {
          nodeName: "BODY",
          ownerDocument: document
        },
        collapse: () => jest.fn()
      });

      const addRange = jest.fn();
      (window as { getSelection: () => void }).getSelection = () => {
        return {
          removeAllRanges: () => {},
          addRange: addRange
        };
      };

      keyboard.SetFocusOnEndField(target);

      expect(addRange).toHaveBeenCalled();
    });
  });
});
