import { shallow } from "enzyme";
import React from 'react';
import PreferencesPassword from './preferences-password';

describe("PreferencesPassword", () => {
  it("renders", () => {
    shallow(<PreferencesPassword />)
  });
});