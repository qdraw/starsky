import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import { INavigateState } from '../../../interfaces/INavigateState';
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
    it("scroll to state with filePath [item exist]", () => {
      var scrollTo = jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      globalHistory.location.state = {
        filePath: exampleData[0].filePath
      } as INavigateState;
      jest.useFakeTimers();

      mount(<ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />, { attachTo: (window as any).domNode });

      act(() => {
        jest.advanceTimersByTime(100);
      });

      expect(scrollTo).toBeCalled();
      expect(scrollTo).toBeCalledWith({ "top": 0 });

      jest.clearAllTimers();
    });

  });
});