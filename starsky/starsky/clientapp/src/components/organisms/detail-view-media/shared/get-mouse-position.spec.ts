import { GetMousePosition } from "./get-mouse-position";

describe("GetMousePosition function", () => {
  it("should return the correct mouse position when all properties are valid", () => {
    const event = {
      target: {
        offsetParent: {
          offsetLeft: 5
        },
        offsetLeft: 10,
        offsetWidth: 100
      },
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(0.35); // (50 - (10 + 5)) / 100
  });

  it("should return 0 when target element or offset parent is falsy", () => {
    const event = {
      target: null,
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(NaN);
  });

  it("should return 0 when offsetParent is falsy", () => {
    const targetElement = document.createElement("progress");
    const event = {
      target: targetElement,
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(NaN);
  });

  it("should return 0.5 when offsetParent offsetLeft is falsy", () => {
    const event = {
      target: {
        offsetParent: {
          offsetLeft: null
        },
        offsetLeft: null,
        offsetWidth: 100
      },
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(0.5);
  });

  it("should return NaN when offsetParent offsetLeft is NaN", () => {
    const event = {
      target: {
        offsetParent: {
          offsetLeft: NaN
        },
        offsetLeft: 10,
        offsetWidth: 100
      },
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(NaN);
  });

  it("should return 0.6 when offsetParent offsetLeft is negative", () => {
    const event = {
      target: {
        offsetParent: {
          offsetLeft: -5
        },
        offsetLeft: -5,
        offsetWidth: 100
      },
      pageX: 50
    } as unknown as MouseEvent;

    const result = GetMousePosition(event);

    expect(result).toBe(0.6);
  });
});
