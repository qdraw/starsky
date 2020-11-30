const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock;

import Adapter from '@wojtekmaj/enzyme-adapter-react-17';
import Enzyme from "enzyme";

Enzyme.configure({ adapter: new Adapter() });