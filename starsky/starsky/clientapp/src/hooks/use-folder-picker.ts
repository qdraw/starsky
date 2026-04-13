import { useCallback } from "react";

/**
 * Hook to handle folder selection using native app capabilities
 * Uses the injected Promise-based native bridge only
 */
export function useFolderPicker() {
  const isNativeApp = useCallback((): boolean => {
    return !!window.__starskyNative?.selectFolder;
  }, []);

  /**
   * Request folder selection from native app
   * Returns a promise that resolves with { path, bookmark }
   */
  const requestFolderSelection = useCallback(
    async (timeoutMs = 30000): Promise<{ path: string | null; bookmark: string | null }> => {
      const nativeBridge = window.__starskyNative;
      if (nativeBridge && typeof nativeBridge.selectFolder === "function") {
        try {
          const result = await nativeBridge.selectFolder(timeoutMs);
          return { path: result?.path ?? null, bookmark: result?.bookmark ?? null };
        } catch {
          return { path: null, bookmark: null };
        }
      }

      // Not running inside native app
      return { path: null, bookmark: null };
    },
    []
  );

  return { isNativeApp, requestFolderSelection };
}
