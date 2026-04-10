import { useState } from "react";
import { IBatchRenameResult } from "../../../../interfaces/IBatchRename";

export interface IFileRenameState {
  shouldRename: boolean;
  setShouldRename: React.Dispatch<React.SetStateAction<boolean>>;
  renamePreview: IBatchRenameResult[];
  setRenamePreview: React.Dispatch<React.SetStateAction<IBatchRenameResult[]>>;
  isLoadingRename: boolean;
  setIsLoadingRename: React.Dispatch<React.SetStateAction<boolean>>;
  isExecutingRename: boolean;
  setIsExecutingRename: React.Dispatch<React.SetStateAction<boolean>>;
  renameError: string | null;
  setRenameError: React.Dispatch<React.SetStateAction<string | null>>;
  reset: () => void;
}

export function useFileRenameState(): IFileRenameState {
  const [shouldRename, setShouldRename] = useState(true);
  const [renamePreview, setRenamePreview] = useState<IBatchRenameResult[]>([]);
  const [isLoadingRename, setIsLoadingRename] = useState(false);
  const [isExecutingRename, setIsExecutingRename] = useState(false);
  const [renameError, setRenameError] = useState<string | null>(null);

  const reset = () => {
    setShouldRename(true);
    setRenamePreview([]);
    setIsLoadingRename(false);
    setIsExecutingRename(false);
    setRenameError(null);
  };

  return {
    shouldRename,
    setShouldRename,
    renamePreview,
    setRenamePreview,
    isLoadingRename,
    setIsLoadingRename,
    isExecutingRename,
    setIsExecutingRename,
    renameError,
    setRenameError,
    reset
  };
}
