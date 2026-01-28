import { useEffect } from "react";

export function useLoadTimezones(currentStep: string, shouldLoad: boolean, onLoad: () => void) {
  useEffect(() => {
    if (currentStep === "timezone" && shouldLoad) {
      onLoad();
    }
  }, [currentStep, shouldLoad, onLoad]);
}
