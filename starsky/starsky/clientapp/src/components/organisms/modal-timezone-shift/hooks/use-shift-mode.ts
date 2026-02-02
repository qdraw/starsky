import { useState } from "react";

export type ShiftMode =
  | "mode-selection"
  | "offset"
  | "timezone"
  | "file-rename-timezone"
  | "file-rename-offset";

export function useShiftMode() {
  const [currentStep, setCurrentStep] = useState<ShiftMode>("mode-selection");

  const handleBack = () => {
    if (currentStep === "offset" || currentStep === "timezone") {
      setCurrentStep("mode-selection");
    }
    if (currentStep === "file-rename-timezone") {
      setCurrentStep("timezone");
    }
    if (currentStep === "file-rename-offset") {
      setCurrentStep("offset");
    }
  };

  const handleModeSelect = (mode: "offset" | "timezone") => {
    setCurrentStep(mode);
  };

  const reset = () => {
    setCurrentStep("mode-selection");
  };

  return {
    currentStep,
    setCurrentStep,
    handleBack,
    handleModeSelect,
    reset
  };
}
