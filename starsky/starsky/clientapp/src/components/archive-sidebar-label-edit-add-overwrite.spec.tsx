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
  describe("with context", () => {

    var useContextSpy: jest.SpyInstance;

    beforeEach(() => {
      // is used in multiple ways
      // use this: ==> import * as AppContext from '../contexts/archive-context';
      useContextSpy = jest
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

    });

    afterEach(() => {
      // and clean your room afterwards
      useContextSpy.mockClear()
    });

    it("isReadOnly: false", () => {

      const mainElement = shallow(<ArchiveSidebarLabelEditAddOverwrite />);

      var formControl = mainElement.find('.form-control');

      // there are 3 classes [title,info,description]
      formControl.forEach(element => {
        expect(element.props()["contentEditable"]).toBeTruthy();
      });

      // if there is no contentEditable it should fail
      expect(formControl.length).toBeGreaterThanOrEqual(3);

    });

    // it("click", () => {
    //   const mainElement = shallow(<ArchiveSidebarLabelEditAddOverwrite />);
    //   var formControl = mainElement.find('.form-control');
    //   formControl.forEach(element => {
    //     // element.simulate('change', { target: { value: 'Hello' } })
    //     element.simulate('keydown', { keyCode: 49 });
    //     // element.props().value = "foo";
    //   });
    //   console.log(mainElement.html());
    // });

  });




});