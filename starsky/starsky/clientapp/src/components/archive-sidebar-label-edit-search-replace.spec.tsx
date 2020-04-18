import { globalHistory } from '@reach/router';
import { mount, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as AppContext from '../contexts/archive-context';
import { IArchive } from '../interfaces/IArchive';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import ArchiveSidebarLabelEditSearchReplace from './archive-sidebar-label-edit-search-replace';
import FormControl from './form-control';
import Notification from './notification';

describe("ArchiveSidebarLabelEditSearchReplace", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarLabelEditSearchReplace />)
  });

  it("isReadOnly: true", () => {
    const mainElement = shallow(<ArchiveSidebarLabelEditSearchReplace />);

    var formControl = mainElement.find(FormControl);

    // there are 3 classes [title,info,description]
    formControl.forEach(element => {
      expect(element.props()["contentEditable"]).toBeFalsy();
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
        state: { isReadOnly: false, fileIndexItems: [{ fileName: 'test.jpg', parentDirectory: '/' }] } as IArchive,
        dispatch: jest.fn(),
      } as AppContext.IArchiveContext;

      jest.mock('@reach/router', () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn(),
      }));

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

      const mainElement = shallow(<ArchiveSidebarLabelEditSearchReplace />);

      var formControl = mainElement.find(FormControl);

      // there are 3 classes [title,info,description] 
      // but those exist 2 times!
      formControl.forEach(element => {
        expect(element.props()["contentEditable"]).toBeTruthy();
      });

      // if there is no contentEditable it should fail
      // double amount of classes
      expect(formControl.length).toBeGreaterThanOrEqual(6);

    });

    it('Should change value when onChange was called', () => {

      const component = mount(<ArchiveSidebarLabelEditSearchReplace />);

      // update component
      component.find('[data-name="tags"]').getDOMNode().textContent = "a";

      // now press a key
      component.find('[data-name="tags"]').simulate('input', { key: 'a' });

      var className = component.find('.btn.btn--default').getDOMNode().className;
      expect(className).toBe('btn btn--default')
    });

    it('click replace', () => {
      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [{ fileName: 'test.jpg', parentDirectory: '/' }] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(connectionDefault);
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const component = mount(<ArchiveSidebarLabelEditSearchReplace />);

      // update component + now press a key
      component.find('[data-name="tags"]').getDOMNode().textContent = "a";
      component.find('[data-name="tags"]').simulate('input', { key: 'a' });

      component.find('.btn.btn--default').simulate('click');


      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith(new UrlQuery().prefix + "/api/replace", "f=%2F%2Ftest.jpg&collections=true&fieldName=tags&search=a&replace=");
    });

    it('click update | read only', async () => {

      jest.spyOn(FetchPost, 'default').mockReset();

      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [{
          fileName: 'test.jpg',
          parentDirectory: '/',
          tags: 'test1, readonly',
          status: IExifStatus.ReadOnly
        }] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(connectionDefault);
      jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const component = mount(<ArchiveSidebarLabelEditSearchReplace />);

      act(() => {
        // update component + now press a key
        component.find('[data-name="tags"]').getDOMNode().textContent = "a";
        component.find('[data-name="tags"]').simulate('input', { key: 'a' });
      });

      // need to await to contain dispatchedValues
      await act(async () => {
        await component.find('.btn.btn--default').simulate('click');
      });

      // force update to get the right state
      component.update();

      expect(component.exists(Notification)).toBeTruthy();

      act(() => {
        component.unmount();
      });

    });

  });
});
