import { useEffect } from "react";

export function useResetOnClose(isOpen: boolean, onReset: () => void) {
  useEffect(() => {
    if (!isOpen) {
      onReset();
    }
  }, [isOpen, onReset]);
}
