import { useState } from "react";

export type ShiftMode = "mode-selection" | "offset" | "timezone";

export function useShiftMode() {
  const [currentStep, setCurrentStep] = useState<ShiftMode>("mode-selection");

  const handleBack = () => {
    if (currentStep === "offset" || currentStep === "timezone") {
      setCurrentStep("mode-selection");
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
