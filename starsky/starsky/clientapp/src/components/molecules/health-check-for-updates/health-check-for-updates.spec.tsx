import { shallow } from 'enzyme';
import React from 'react';
import HealthCheckForUpdates from './health-check-for-updates';

describe("HealthCheckForUpdates", () => {

  it("renders (without state component)", () => {
    shallow(<HealthCheckForUpdates />)
  });

  describe("with Context", () => {
    it("Ok ", () => {
    });

  });
});