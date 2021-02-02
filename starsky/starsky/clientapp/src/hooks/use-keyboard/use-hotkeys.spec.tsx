import { Keyboard } from "../../shared/keyboard";
import { mountReactHook } from "../___tests___/test-hook";
import useHotKeys, { IHotkeysKeyboardEvent } from "./use-hotkeys";

describe("useHotKeys", () => {
  describe("should ignore", () => {
    it("missing input", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [{}, callback]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        altKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });

    it("should ignore when keyboard is in form", () => {
      const callback = jest.fn();

      jest
        .spyOn(Keyboard.prototype, "isInForm")
        .mockImplementationOnce(() => true);

      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          altKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        altKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });
  });

  describe("pressing key with q", () => {
    it("should return when pressing alt q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          altKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        altKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalled();
      test.componentMount.unmount();
    });

    it("should return when pressing control q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        ctrlKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalled();
      test.componentMount.unmount();
    });

    it("should return when pressing command/meta q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          metaKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        metaKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalled();
      test.componentMount.unmount();
    });

    it("should return when pressing shift q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          shiftKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        shiftKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalled();
      test.componentMount.unmount();
    });

    it("should not return when pressing  q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          shiftKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q"
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });
  });
});
