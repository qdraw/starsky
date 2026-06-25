import { useCallback } from "react";

/**
 * Hook to handle folder selection using native app capabilities
 * Uses the injected Promise-based native bridge only
 */
export function useFolderPicker() {
  const isNativeApp = useCallback((): boolean => {
    return !!(globalThis as any).__starskyNative?.selectFolder;
  }, []);

  /**
   * Request folder selection from native app
   * Returns a promise that resolves with { path, bookmark }
   */
  const requestFolderSelection = useCallback(
    (arg?: number | ((path: string | null, bookmark: string | null) => void)) => {
      const timeoutMs = typeof arg === "number" ? arg : 30000;

      const nativeBridge = (globalThis as any).__starskyNative as any;

      // If native provides a promise-based selectFolder, use it directly
      if (nativeBridge && typeof nativeBridge.selectFolder === "function") {
        const promise = (async () => {
          try {
            const result = await nativeBridge.selectFolder(timeoutMs);
            return { path: result?.path ?? null, bookmark: result?.bookmark ?? null };
          } catch {
            return { path: null, bookmark: null };
          }
        })();

        if (typeof arg === "function") {
          // callback style
          promise.then((r) => arg(r.path, r.bookmark));
          return;
        }

        return promise;
      }

      // Fallback path: use resolver map called by native via
      // window.__starskyNative._resolveFolderPick(requestId, path, bookmarkBase64)
      if (!(globalThis as any).__starskyNative) {
        if (typeof arg === "function") {
          arg(null, null);
          return;
        }
        return Promise.resolve({ path: null, bookmark: null });
      }

      const bridge: any = (globalThis as any).__starskyNative;
      bridge._folderPickResolvers = bridge._folderPickResolvers || {};

      if (typeof bridge._resolveFolderPick !== "function") {
        bridge._resolveFolderPick = (
          requestId: string,
          path: string | null,
          bookmarkBase64: string | null
        ) => {
          // Prefer resolving by requestId
          const resolver = bridge._folderPickResolvers?.[requestId];
          if (resolver) {
            try {
              resolver.resolve({ path: path ?? null, bookmark: bookmarkBase64 ?? null });
            } catch (e) {
              // ignore
            }
            delete bridge._folderPickResolvers[requestId];
            return;
          }

          // If no resolver matched the id, resolve the first pending resolver (legacy convenience)
          const keys = Object.keys(bridge._folderPickResolvers || {});
          if (keys.length > 0) {
            const firstKey = keys[0];
            const firstResolver = bridge._folderPickResolvers[firstKey];
            try {
              firstResolver.resolve({ path: path ?? null, bookmark: bookmarkBase64 ?? null });
            } catch (e) {
              // ignore
            }
            delete bridge._folderPickResolvers[firstKey];
          }
        };
      }

      const requestId = `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;

      const promise = new Promise<{ path: string | null; bookmark: string | null }>((resolve) => {
        bridge._folderPickResolvers[requestId] = { resolve };
        window.setTimeout(() => {
          if (bridge._folderPickResolvers?.[requestId]) {
            resolve({ path: null, bookmark: null });
            delete bridge._folderPickResolvers[requestId];
          }
        }, timeoutMs);
      });

      if (typeof arg === "function") {
        promise.then((r) => arg(r.path, r.bookmark));
        return;
      }

      return promise;
    },
    []
  );

  return { isNativeApp, requestFolderSelection };
}
