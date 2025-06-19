export const mockObserve = jest.fn();
export const mockUnobserve = jest.fn();
export const mockDisconnect = jest.fn();

let callback: IntersectionObserverCallback;

export function triggerIntersection(entries: Partial<IntersectionObserverEntry>[]) {
  if (callback) {
    callback(entries as IntersectionObserverEntry[], {} as IntersectionObserver);
  }
}

global.IntersectionObserver = jest.fn((_cb, _options) => {
  callback = _cb;
  return {
    observe: mockObserve,
    unobserve: mockUnobserve,
    disconnect: mockDisconnect
  };
});
