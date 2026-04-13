// Augment the global Window interface for native webview messaging
export {};

declare global {
  interface StarskyFolderSelectedDetail {
    path: string | null;
    bookmark: string | null;
  }

  interface Window {
    // Promise-based injected bridge (added by the mac app at document-start)
    __starskyNative?: {
      // selectFolder(timeoutMs?) => Promise<{ path, bookmark }>
      selectFolder?: (
        timeoutMs?: number
      ) => Promise<{ path: string | null; bookmark: string | null }>;
      // low-level forwarders (optional)
      filePicker?: (payload: unknown) => boolean;
      consolePost?: (obj: unknown) => boolean;
    } & Record<string, unknown>;

    // test helper: optional dom node used by some tests to attach elements
    domNode?: HTMLDivElement | null;
    // test/debug helpers used across the codebase
    debug?: boolean;
    // storybook / renderer flags
    isElectron?: boolean | null;

    // event typing helper: addEventListener('starskyFolderSelected', (e: CustomEvent<StarskyFolderSelectedDetail>) => { ... })
    addEventListener(
      type: "starskyFolderSelected",
      listener: (ev: CustomEvent<StarskyFolderSelectedDetail>) => void
    ): void;
  }
}
