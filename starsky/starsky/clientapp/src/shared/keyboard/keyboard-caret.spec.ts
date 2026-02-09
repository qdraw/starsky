import { Keyboard } from "./keyboard";

describe("Keyboard.setCaretToEnd", () => {
  let keyboard: Keyboard;

  beforeEach(() => {
    keyboard = new Keyboard();
  });

  it("returns null if event is undefined", () => {
    expect(keyboard.setCaretToEnd(undefined)).toBeNull();
  });

  it("returns null if not in form", () => {
    // Mock event with target not in form
    const event = {
      target: { className: "not-form", isContentEditable: false }
    } as unknown as KeyboardEvent;
    jest.spyOn(keyboard, "isInForm").mockReturnValue(false);
    expect(keyboard.setCaretToEnd(event)).toBeNull();
  });

  it("returns true and sets caret for contenteditable", (exit) => {
    // Mock Range API for jsdom
    const originalCreateRange = document.createRange;
    const mockRange = {
      selectNodeContents: jest.fn(),
      collapse: jest.fn(),
      setStart: jest.fn(),
      setEnd: jest.fn()
      // ... add any other methods you need
    };
    (document as unknown as { createRange: () => void }).createRange = () => mockRange;

    // Create a contenteditable div and add to DOM
    const mockDiv = document.createElement("div");
    mockDiv.contentEditable = "true";
    mockDiv.className = "form-control";
    mockDiv.innerText = "test";
    Object.defineProperty(mockDiv, "isContentEditable", {
      value: true,
      configurable: true
    });

    document.body.appendChild(mockDiv);
    // Create a real KeyboardEvent
    const event = new KeyboardEvent("keydown", { bubbles: true });

    mockDiv.addEventListener("keydown", (event) => {
      expect(keyboard.setCaretToEnd(event)).toBe(true);
      (document as any).createRange = originalCreateRange;
      exit();
    });

    mockDiv.dispatchEvent(event);
  });

  it("returns false for contenteditable when no getSelection", (exit) => {
    // Mock event with contenteditable target
    const mockDiv = document.createElement("div");
    mockDiv.contentEditable = "true";
    mockDiv.className = "form-control"; // isInForm should return true
    mockDiv.innerText = "test";

    jest.spyOn(window, "getSelection").mockImplementationOnce(() => false as unknown as Selection);
    Object.defineProperty(mockDiv, "isContentEditable", {
      value: true,
      configurable: true
    });

    document.body.appendChild(mockDiv);
    // Create a real KeyboardEvent
    const event = new KeyboardEvent("keydown", { bubbles: true });

    mockDiv.addEventListener("keydown", (event) => {
      expect(keyboard.setCaretToEnd(event)).toBe(false);
      exit();
    });

    mockDiv.dispatchEvent(event);
  });

  it("returns false for non-contenteditable element", () => {
    // Mock event with non-contenteditable target
    const input = document.createElement("input");
    input.value = "test";
    document.body.appendChild(input);
    const event = {
      target: input
    } as unknown as KeyboardEvent;
    jest.spyOn(keyboard, "isInForm").mockReturnValue(true);
    expect(keyboard.setCaretToEnd(event)).toBe(false);
    document.body.removeChild(input);
  });
});
