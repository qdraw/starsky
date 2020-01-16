import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as AppContext from '../contexts/archive-context';
import { newIArchive } from '../interfaces/IArchive';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';

describe("ArchiveSidebarColorClass", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />)
  });

  describe("mount object (mount= select is child element)", () => {
    var wrapper = mount(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />);

    it("colorclass--select class exist", () => {
      expect(wrapper.exists('.colorclass--select')).toBeTruthy()
    });

    it("not disabled", () => {
      expect(wrapper.exists('.disabled')).toBeFalsy()
    });

    it("Fire event when clicked", () => {


      // Warning: An update to null inside a test was not wrapped in act(...)


      // is used in multiple ways
      // use this: ==> import * as AppContext from '../contexts/archive-context';
      var useContextSpy = jest
        .spyOn(React, 'useContext')
        .mockImplementation(() => contextValues);

      var dispatch = jest.fn();
      const contextValues = {
        state: newIArchive(),
        dispatch,
      } as AppContext.IArchiveContext;

      // use this: ==> import * as AppContext from '../contexts/archive-context';
      // var useContext = jest
      //   .spyOn(AppContext, 'useArchiveContext')
      //   .mockImplementation(() => contextValues);

      jest.mock('@reach/router', () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn(),
      }));


      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test.jpg");
      });

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const element = mount(<ArchiveSidebarColorClass isReadOnly={false} fileIndexItems={newIFileIndexItemArray()} />);

      // Make sure that the element exist in the first place
      expect(element.find('a.colorclass--1')).toBeTruthy();

      element.find('button.colorclass--1').simulate("click");

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);

      useContextSpy.mockClear()

    });
  });
});

