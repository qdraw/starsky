import { shallow } from "enzyme";
import React from 'react';
import PreferencesPage from './preferences-page';

describe("PreferencesPage", () => {
  it("renders", () => {
    shallow(<PreferencesPage />)
  });
});