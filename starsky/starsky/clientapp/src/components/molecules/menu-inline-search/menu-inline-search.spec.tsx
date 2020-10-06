import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../../../hooks/use-fetch';
import { IConnectionDefault, newIConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as ArrowKeyDown from './arrow-key-down';
import MenuInlineSearch from './menu-inline-search';

describe("Menu.SearchBar", () => {

  it("renders", () => {
    shallow(<MenuInlineSearch />)
  });

  describe("with Context", () => {
    it("focus", () => {

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return { ...newIConnectionDefault(), statusCode: 200, };
      }).mockImplementationOnce(() => {
        return { ...newIConnectionDefault(), statusCode: 200, };
      })

      var menuBar = mount(<MenuInlineSearch />);

      // default
      expect(menuBar.find('label').hasClass('icon-addon--search'))

      menuBar.find('input').simulate('focus');
      expect(menuBar.find('label').hasClass('icon-addon--search-focus'))

      menuBar.unmount();
    });
    it("blur", () => {

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      var menuBar = mount(<MenuInlineSearch />);

      // go to focus
      menuBar.find('input').simulate('focus');
      expect(menuBar.find('label').hasClass('icon-addon--search-focus'))

      // go to blur
      menuBar.find('input').simulate('blur');
      expect(menuBar.find('label').hasClass('icon-addon--search'))

      menuBar.unmount();
    });

    var suggestionsExample = { statusCode: 200, data: ["suggest1", "suggest2"] } as IConnectionDefault

    it("suggestions", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)

      var callback = jest.fn();
      var menuBar = mount(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = "test"
      menuBar.find('input').simulate('change');

      var results = menuBar.find('.menu-item--results > button')
      expect(results.first().text()).toBe("suggest1");
      expect(results.last().text()).toBe("suggest2");
      expect(callback).toBeCalledTimes(0);
      menuBar.unmount();
    });

    it("suggestions and click button", () => {

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)


      var callback = jest.fn();
      var menuBar = mount(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = "test"
      menuBar.find('input').simulate('change');

      var results = menuBar.find('.menu-item--results > button');
      results.first().simulate("click");

      expect(callback).toBeCalled();
      menuBar.unmount();

    });

    it("reset suggestions after change to nothing", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())

      var callback = jest.fn();
      var menuBar = mount(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = "test"
      menuBar.find('input').simulate('change');

      // and change it back
      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = ""
      menuBar.find('input').simulate('change');

      var results = menuBar.find('.menu-item--default').first();

      expect(results).toBeTruthy();

      expect(callback).toBeCalledTimes(0);
      menuBar.unmount();
    });

    it("ArrowKeyDown called", () => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default')
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => suggestionsExample)
        .mockImplementationOnce(() => suggestionsExample)

      var callback = jest.fn();
      var menuBar = mount(<MenuInlineSearch defaultText={"tes"} callback={callback} />);

      var arrowKeyDownSpy = jest.spyOn(ArrowKeyDown, 'default').mockImplementationOnce(() => { });

      (menuBar.find('input').getDOMNode() as HTMLInputElement).value = "test"
      menuBar.find('input').simulate('keydown');

      expect(arrowKeyDownSpy).toBeCalled();

      menuBar.unmount();
    });

  });

});