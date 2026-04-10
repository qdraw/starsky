import { useState } from "react";
import { IExifTimezoneCorrectionResultContainer } from "../../../../interfaces/ITimezone";

export interface IPreviewState {
  preview: IExifTimezoneCorrectionResultContainer;
  setPreview: React.Dispatch<React.SetStateAction<IExifTimezoneCorrectionResultContainer>>;
  isLoadingPreview: boolean;
  setIsLoadingPreview: React.Dispatch<React.SetStateAction<boolean>>;
  isExecuting: boolean;
  setIsExecuting: React.Dispatch<React.SetStateAction<boolean>>;
  error: string | null;
  setError: React.Dispatch<React.SetStateAction<string | null>>;
  previewReset: () => void;
}

export function usePreviewState(): IPreviewState {
  const [preview, setPreview] = useState<IExifTimezoneCorrectionResultContainer>({
    offsetData: [],
    timezoneData: []
  });
  const [isLoadingPreview, setIsLoadingPreview] = useState(false);
  const [isExecuting, setIsExecuting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const previewReset = () => {
    setPreview({ offsetData: [], timezoneData: [] });
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
    previewReset
  };
}
