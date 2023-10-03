import "@testing-library/jest-dom";
import { configure } from "@testing-library/react";
import "isomorphic-fetch";

// Mock IntersectionObserver
class IntersectionObserver {
  observe = jest.fn();
  disconnect = jest.fn();
  unobserve = jest.fn();
}

Object.defineProperty(window, "IntersectionObserver", {
  writable: true,
  configurable: true,
  value: IntersectionObserver
});

Object.defineProperty(global, "IntersectionObserver", {
  writable: true,
  configurable: true,
  value: IntersectionObserver
});

window.scrollTo = jest.fn();

configure({ testIdAttribute: "data-test" });
