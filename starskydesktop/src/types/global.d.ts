declare global {
  interface Window {
    /** APIs for Electron IPC */
    ipcAPI?: any
  }
}

// Makes TS sees this as an external modules so we can extend the global scope.
export { };
