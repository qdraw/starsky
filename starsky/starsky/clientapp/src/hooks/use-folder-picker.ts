import { useCallback } from "react";

/**
 * Hook to handle folder selection using native app capabilities
 * Detects macOS WKWebView or Windows WebView2
 * Falls back to a callback if not in a native app
 */
export function useFolderPicker() {
  /**
   * Check if running in a native app (macOS WKWebView or Windows WebView2)
   */
  const isNativeApp = useCallback((): boolean => {
    return !!(window.webkit?.messageHandlers?.filePicker || window.chrome?.webview);
  }, []);

  /**
   * Request folder selection from native app
   * @param onFolderSelected Callback when folder is selected or when not in native app
   */
  const requestFolderSelection = useCallback(
    (onFolderSelected: (folderPath: string | null) => void) => {
      // macOS WKWebView
      if (window.webkit?.messageHandlers?.filePicker) {
        // Register listener for folder selection
        window.onFolderSelected = (folderPath: string | null) => {
          onFolderSelected(folderPath);
          window.onFolderSelected = undefined;
        };

        window.webkit.messageHandlers.filePicker.postMessage({
          action: "selectFolder"
        });
        return;
      }

      // Windows WebView2
      if (window.chrome?.webview) {
        // Register listener for folder selection
        window.onFolderSelected = (folderPath: string | null) => {
          onFolderSelected(folderPath);
          window.onFolderSelected = undefined;
        };

        window.chrome.webview.postMessage({
          action: "selectFolder"
        });
        return;
      }

      // Fallback: not running inside native app
      console.warn("Not running inside native app");
      onFolderSelected(null);
    },
    []
  );

  return {
    isNativeApp,
    requestFolderSelection
  };
}
