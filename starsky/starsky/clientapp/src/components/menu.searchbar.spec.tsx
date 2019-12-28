import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../hooks/use-fetch';
import * as useLocation from '../hooks/use-location';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import MenuSearchBar from './menu.searchbar';

describe("Menu.SearchBar", () => {

  it("renders", () => {
    shallow(<MenuSearchBar />)
  });

  describe("with Context", () => {
    xit("focus", () => {

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      var menuBar = mount(<MenuSearchBar />);

      expect(menuBar.find('label').hasClass('icon-addon--search'))
      menuBar.find('input').simulate('focus');
      expect(menuBar.find('label').hasClass('icon-addon--search-focus'))

      menuBar.unmount();
    });

    xit("ttt", () => {
      jest.spyOn(React, 'useState').mockImplementationOnce(() => {
        return [jest.fn(), jest.fn()]
      }).mockImplementationOnce(() => {
        return [jest.fn(), jest.fn()]
      })

      var navigateSpy = jest.fn();
      var locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      }).mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      });
    })

    it("suggestions", () => {

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => {
          return newIConnectionDefault()
        })
        .mockImplementationOnce(() => {
          return { statusCode: 200, data: ["suggest1", "suggest2"] } as IConnectionDefault;
        })
        .mockImplementationOnce(() => {
          return newIConnectionDefault()
        })
      var menuBar = mount(<MenuSearchBar defaultText={"te"} />);

      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = "test"
      menuBar.find('input').simulate('change');

      var results = menuBar.find('.menu-item--results > button')
      expect(results.first().text()).toBe("suggest1");
      expect(results.last().text()).toBe("suggest2");

    });
  });

});