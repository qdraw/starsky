import { shallow } from "enzyme";
import React from 'react';
// import { act } from 'react-dom/test-utils';
// import * as AppContext from '../contexts/archive-context';
// import { IArchive } from '../interfaces/IArchive';
// import { globalHistory } from '@reach/router';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarLabelEdit />)
  });
  // describe("with context", () => {

  //   var useContextSpy: jest.SpyInstance;

  //   beforeEach(() => {
  //     // is used in multiple ways
  //     // use this: ==> import * as AppContext from '../contexts/archive-context';
  //     useContextSpy = jest
  //       .spyOn(React, 'useContext')
  //       .mockImplementation(() => contextValues);

  //     const contextValues = {
  //       state: { isReadOnly: false } as IArchive,
  //       dispatch: jest.fn(),
  //     } as AppContext.IArchiveContext;

  //     jest.mock('@reach/router', () => ({
  //       navigate: jest.fn(),
  //       globalHistory: jest.fn(),
  //     }))

  //     act(() => {
  //       // to use with: => import { act } from 'react-dom/test-utils';
  //       globalHistory.navigate("/?select=test.jpg");
  //     });

  //   });

  //   afterEach(() => {
  //     // and clean your room afterwards
  //     useContextSpy.mockClear()
  //   });

  //   it("toggle isReplaceMode", () => {
  //     var mainElement = shallow(<ArchiveSidebarLabelEdit />);
  //     var switchButton = mainElement.find('SwitchButton');
  //     var right = switchButton.find("#switch_right_0");
  //     console.log(right);

  //     console.log(switchButton.html());

  //   });

  // });
});