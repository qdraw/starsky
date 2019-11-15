
import { globalHistory } from '@reach/router';
import { shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

describe("archive-sidebar-selection-list", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />)
  });

  describe("with select state", () => {

    beforeEach(() => {
      jest.mock('@reach/router', () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn(),
      }))

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test.jpg");
      });

    });

    var items = [{ fileName: 'test.jpg', parentDirectory: '/' }, { fileName: 'to-select.jpg', parentDirectory: '/' }] as IFileIndexItem[]


    it("with items, check first item", () => {
      const component = shallow(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      var first = component.find('ul li').first();
      expect(first.text()).toBe("test.jpg")
    });

    it("toggleSelection", () => {

      const component = shallow(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      var spy = jest.spyOn(URLPath.prototype, 'toggleSelection');

      var first = component.find('ul li').first();
      first.find('.close').simulate('click');

      expect(spy).toBeCalledTimes(1)

      spy.mockClear();
    });

    it("allSelection", () => {

      const component = shallow(<ArchiveSidebarSelectionList fileIndexItems={items} />);
      var allSelectionButton = component.find('[data-test="allSelection"]')

      var spy = jest.spyOn(URLPath.prototype, 'GetAllSelection');

      allSelectionButton.simulate('click');

      expect(spy).toBeCalledTimes(1)

      spy.mockClear();
    });

  });
});