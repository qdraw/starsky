// this file is excluded from the build in vite.config.ts
export const mockObserve = jest.fn();
export const mockUnobserve = jest.fn();
export const mockDisconnect = jest.fn();

let callback: IntersectionObserverCallback;

export function triggerIntersection(entries: Partial<IntersectionObserverEntry>[]) {
  if (callback) {
    callback(entries as IntersectionObserverEntry[], {} as IntersectionObserver);
  }
}

globalThis.IntersectionObserver = jest.fn((_cb, _options) => {
  callback = _cb;
  return {
    observe: mockObserve,
    unobserve: mockUnobserve,
    disconnect: mockDisconnect,
    root: null,
    rootMargin: "",
    thresholds: [],
    takeRecords: jest.fn()
  };
});
