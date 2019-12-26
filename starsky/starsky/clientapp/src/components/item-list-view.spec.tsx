import { mount, shallow } from 'enzyme';
import React from 'react';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import ItemListView from './item-list-view';

describe("ItemListView", () => {

  it("renders (without state component)", () => {
    shallow(<ItemListView fileIndexItems={newIFileIndexItemArray()} colorClassUsage={[]} />)
  });

  describe("with Context", () => {

    var exampleData = [
      { fileName: 'test.jpg', filePath: '/test.jpg' }
    ] as IFileIndexItem[]

    it("search with data-filepath in child element", () => {
      var list = mount(<ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />)
      var query = '[data-filepath="' + exampleData[0].filePath + '"]';

      expect(list.exists(query)).toBeTruthy();
    });

  });
});