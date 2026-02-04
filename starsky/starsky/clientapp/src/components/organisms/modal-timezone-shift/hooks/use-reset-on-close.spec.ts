import { renderHook } from "@testing-library/react";
import { useResetOnClose } from "./use-reset-on-close";

describe("useResetOnClose", () => {
  it("calls onReset when isOpen becomes false", () => {
    const onReset = jest.fn();
    const { rerender } = renderHook(({ isOpen }) => useResetOnClose(isOpen, onReset), {
      initialProps: { isOpen: true }
    });
    expect(onReset).not.toHaveBeenCalled();
    rerender({ isOpen: false });
    expect(onReset).toHaveBeenCalledTimes(1);
  });

  it("does not call onReset when isOpen stays true", () => {
    const onReset = jest.fn();
    const { rerender } = renderHook(({ isOpen }) => useResetOnClose(isOpen, onReset), {
      initialProps: { isOpen: true }
    });
    rerender({ isOpen: true });
    expect(onReset).not.toHaveBeenCalled();
  });
});
