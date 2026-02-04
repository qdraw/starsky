import { renderHook } from "@testing-library/react";
import { act } from "react";
import { useShiftMode } from "./use-shift-mode";

describe("useShiftMode", () => {
  it("should start at mode-selection", () => {
    const { result } = renderHook(() => useShiftMode());
    expect(result.current.currentStep).toBe("mode-selection");
  });

  it("handleModeSelect sets mode", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.handleModeSelect("offset");
    });
    expect(result.current.currentStep).toBe("offset");
    act(() => {
      result.current.handleModeSelect("timezone");
    });
    expect(result.current.currentStep).toBe("timezone");
  });

  it("handleBack returns to mode-selection from offset or timezone", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.handleModeSelect("offset");
    });
    act(() => {
      result.current.handleBack();
    });
    expect(result.current.currentStep).toBe("mode-selection");
    act(() => {
      result.current.handleModeSelect("timezone");
    });
    act(() => {
      result.current.handleBack();
    });
    expect(result.current.currentStep).toBe("mode-selection");
  });

  it("handleBack does nothing if already at mode-selection", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.handleBack();
    });
    expect(result.current.currentStep).toBe("mode-selection");
  });

  it("reset always returns to mode-selection", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.handleModeSelect("offset");
    });
    act(() => {
      result.current.reset();
    });
    expect(result.current.currentStep).toBe("mode-selection");
  });

  it("handleBack goes to timezone from file-rename-timezone", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.setCurrentStep("file-rename-timezone");
    });
    act(() => {
      result.current.handleBack();
    });
    expect(result.current.currentStep).toBe("timezone");
  });

  it("handleBack goes to offset from file-rename-offset", () => {
    const { result } = renderHook(() => useShiftMode());
    act(() => {
      result.current.setCurrentStep("file-rename-offset");
    });
    act(() => {
      result.current.handleBack();
    });
    expect(result.current.currentStep).toBe("offset");
  });
});
