// Augment the global Window interface for native webview messaging
export {};

declare global {
  interface Window {
    // macOS WKWebView message handlers
    webkit?: {
      messageHandlers?: {
        filePicker?: {
          postMessage: (message: Record<string, unknown> | unknown) => void;
        } & Record<string, unknown>;
      } & Record<string, unknown>;
    } & Record<string, unknown>;

    // Windows WebView2
    chrome?: {
      webview?: {
        postMessage: (message: Record<string, unknown> | unknown) => void;
        // other runtime helpers optionally exposed
        addEventListener?: (name: string, listener: (...args: unknown[]) => void) => void;
      } & Record<string, unknown>;
    } & Record<string, unknown>;

    // callback used by native host to send selected folder back to the page
    onFolderSelected?: (folderPath: string | null) => void;
    // test helper: optional dom node used by some tests to attach elements
    domNode?: HTMLDivElement | null;
    // test/debug helpers used across the codebase
    debug?: boolean;
    // storybook / renderer flags
    isElectron?: boolean | null;
  }
}
