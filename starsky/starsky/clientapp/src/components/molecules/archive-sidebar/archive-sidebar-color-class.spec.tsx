import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as AppContext from '../../../contexts/archive-context';
import { newIArchive } from '../../../interfaces/IArchive';
import { PageType } from '../../../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import * as ColorClassSelect from '../color-class-select/color-class-select';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';

describe("ArchiveSidebarColorClass", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarColorClass pageType={PageType.Archive} fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />)
  });

  describe("mount object (mount= select is child element)", () => {
    var wrapper = mount(<ArchiveSidebarColorClass pageType={PageType.Archive} fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />);

    it("colorclass--select class exist", () => {
      expect(wrapper.exists('.colorclass--select')).toBeTruthy()
    });

    it("not disabled", () => {
      expect(wrapper.exists('.disabled')).toBeFalsy()
    });

    it("Fire event when clicked", async () => {


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

      jest.mock('@reach/router', () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn(),
      }));

      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?select=test.jpg");

      var isCalled = false;
      jest.spyOn(ColorClassSelect, 'default').mockImplementationOnce(() => { return <></> })
        .mockImplementationOnce(() => { return <></> })
        .mockImplementationOnce((data) => {
          return <button onClick={() => {
            data.onToggle(1);
            isCalled = true;
          }} className="colorclass--1"></button>
        })

      const element = mount(<ArchiveSidebarColorClass pageType={PageType.Archive} isReadOnly={false} fileIndexItems={newIFileIndexItemArray()} />);

      // Make sure that the element exist in the first place
      expect(element.find('button.colorclass--1')).toBeTruthy();

      await act(async () => {
        await element.find('button.colorclass--1').simulate("click");
      });

      expect(isCalled).toBeTruthy();
      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith({ "colorclass": 1, "select": ["test.jpg"], "type": "update" });

      useContextSpy.mockClear()
    });

  });
});

