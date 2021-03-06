import { Keyboard } from "./keyboard";

describe("keyboard", () => {
  var keyboard = new Keyboard();
  describe("isInForm", () => {
    it("null input", () => {
      // new EventTarget() = not supported in Safari
      var eventTarget: EventTarget = new EventTarget();
      var event = new KeyboardEvent("keydown", {
        keyCode: 37,
        target: eventTarget
      } as KeyboardEventInit);

      var result = keyboard.isInForm(event);

      expect(result).toBeNull();
    });

    it("should ignore form-control", () => {
      let target = document.createElement("div");
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
      let target = document.createElement("div");
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
    it("input", () => {
      var target = document.createElement("div");
      target.className = "test";
      target.innerHTML = "input";

      var setStart = jest.fn();
      (document.createRange as any) = () => ({
        setStart: () => setStart,
        setEnd: () => {},
        commonAncestorContainer: {
          nodeName: "BODY",
          ownerDocument: document
        },
        collapse: () => jest.fn()
      });

      var addRange = jest.fn();
      (window.getSelection as any) = () => {
        return {
          removeAllRanges: () => {},
          addRange: addRange
        };
      };

      keyboard.SetFocusOnEndField(target);

      expect(addRange).toBeCalled();
    });
  });
});
