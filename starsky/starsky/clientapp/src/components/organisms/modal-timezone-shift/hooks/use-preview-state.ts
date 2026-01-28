import { useState } from "react";
import { ITimezoneShiftResult } from "../../../interfaces/ITimezone";

export function usePreviewState() {
  const [preview, setPreview] = useState<ITimezoneShiftResult[]>([]);
  const [isLoadingPreview, setIsLoadingPreview] = useState(false);
  const [isExecuting, setIsExecuting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const reset = () => {
    setPreview([]);
    setError(null);
    setIsLoadingPreview(false);
    setIsExecuting(false);
  };

  return {
    preview,
    setPreview,
    isLoadingPreview,
    setIsLoadingPreview,
    isExecuting,
    setIsExecuting,
    error,
    setError,
    reset
  };
}
