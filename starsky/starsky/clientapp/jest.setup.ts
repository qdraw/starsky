import "@testing-library/jest-dom";
import { configure } from "@testing-library/react";
import "isomorphic-fetch";
import "jest-environment-jsdom";
import "ts-node";

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

globalThis.scrollTo = jest.fn();

Object.defineProperty(global.crypto, "randomUUID", {
  value: jest.fn(() => "123e4567-e89b-12d3-a456-426614174000"),
  writable: true,
  configurable: true
});

configure({ testIdAttribute: "data-test" });
