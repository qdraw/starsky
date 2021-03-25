import { Keyboard } from "../../shared/keyboard";
import { mountReactHook } from "../___tests___/test-hook";
import useHotKeys, { IHotkeysKeyboardEvent } from "./use-hotkeys";

describe("useHotKeys", () => {
  describe("should ignore", () => {
    it("missing input as empty object", () => {
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

    it("missing input as undefined", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [undefined, callback]);

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
    it("should return when pressing alt+q and expect alt+q", () => {
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

    it("should return when pressing control q and expect key:q", () => {
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

    it("should return when pressing command/meta q and expect cmd/meta+q", () => {
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

    it("should return when pressing shift+q and expect shift+q", () => {
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

    it("should not return when pressing q and expect shift+q", () => {
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

    it("should not return when pressing shift+q and expect q only", () => {
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

    it("ctrlKeyOrMetaKey - should return when pressing command/meta with combination option enabled q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true
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

    it("ctrlKeyOrMetaKey - should return when pressing ctrlKey with combination option enabled q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true
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

    it("ctrlKeyOrMetaKey - should return when pessing cmdOrCtlr+shift+q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true,
          shiftKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        shiftKey: true,
        metaKey: true
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalled();
      test.componentMount.unmount();
    });

    it("ctrlKeyOrMetaKey - should not return when pressing shiftKey with combination option enabled q", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true
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

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });

    it("ctrlKeyOrMetaKey - should not return when pressing q only with combination option enabled", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "q",
        metaKey: false,
        ctrlKey: false
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });

    it("ctrlKeyOrMetaKey - should not return when pressing t only with combination option enabled", () => {
      const callback = jest.fn();
      const test = mountReactHook(useHotKeys, [
        {
          key: "q",
          ctrlKeyOrMetaKey: true
        } as IHotkeysKeyboardEvent,
        callback
      ]);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "t",
        metaKey: false,
        ctrlKey: false
      });
      window.dispatchEvent(event);

      expect(callback).toBeCalledTimes(0);
      test.componentMount.unmount();
    });
  });
});
