import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../hooks/use-fetch';
import { newIConnectionDefault } from '../interfaces/IConnectionDefault';
import HealthStatusError from './health-status-error';

describe("ItemListView", () => {

  it("renders (without state component)", () => {
    shallow(<HealthStatusError />)
  });

  describe("with Context", () => {
    it("Ok ", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      });
      var component = mount(<HealthStatusError />);

      expect(component.html()).toBe(null);
    });

    it("Error 500", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return { ...newIConnectionDefault(), statusCode: 500 };
      });
      var component = mount(<HealthStatusError />);

      expect(component.html()).toBe(null);
    });
  });

});
