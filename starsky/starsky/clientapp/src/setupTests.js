import { configure } from "@testing-library/react";

const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn()
};
global.localStorage = localStorageMock;

configure({ testIdAttribute: "data-test" });

// // we are not using testing-library/jest-dom
// import Adapter from "@wojtekmaj/enzyme-adapter-react-17";
// import Enzyme from "enzyme";

// Enzyme.configure({ adapter: new Adapter() });
