import { shallow } from "enzyme";
import React from 'react';
import { Preferences } from './preferences';

describe("Preferences", () => {
  it("renders", () => {
    shallow(<Preferences />)
  });
});