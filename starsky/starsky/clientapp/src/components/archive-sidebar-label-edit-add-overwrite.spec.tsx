import { globalHistory } from '@reach/router';
import { shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as AppContext from '../contexts/archive-context';
import { IArchive } from '../interfaces/IArchive';
import ArchiveSidebarLabelEditAddOverwrite from './archive-sidebar-label-edit-add-overwrite';

describe("ArchiveSidebarLabelEditAddOverwrite", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarLabelEditAddOverwrite />)
  });

  it("isReadOnly: true", () => {
    const mainElement = shallow(<ArchiveSidebarLabelEditAddOverwrite />);

    var formControl = mainElement.find('.form-control');

    // there are 3 classes [title,info,description]
    formControl.forEach(element => {
      var disabled = element.hasClass('disabled');
      expect(disabled).toBeTruthy();
    });

  });

  it("isReadOnly: false", () => {

    // is used in multiple ways
    // use this: ==> import * as AppContext from '../contexts/archive-context';
    var useContextSpy = jest
      .spyOn(React, 'useContext')
      .mockImplementation(() => contextValues);


    const contextValues = {
      state: { isReadOnly: false } as IArchive,
      dispatch: jest.fn(),
    } as AppContext.IArchiveContext;


    jest.mock('@reach/router', () => ({
      navigate: jest.fn(),
      globalHistory: jest.fn(),
    }))

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      globalHistory.navigate("/?select=test.jpg");
    });

    const mainElement = shallow(<ArchiveSidebarLabelEditAddOverwrite />);

    var formControl = mainElement.find('.form-control');

    // there are 3 classes [title,info,description]
    formControl.forEach(element => {
      expect(element.props()["contentEditable"]).toBeTruthy();
    });

    // if there is no contentEditable it should fail
    expect(formControl.length).toBeGreaterThanOrEqual(3);

    // and clean your room afterwards
    useContextSpy.mockClear();

  });


});