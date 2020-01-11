import { globalHistory } from '@reach/router';
import { act } from '@testing-library/react';
import { mount, shallow } from "enzyme";
import React from 'react';
import * as AppContext from '../contexts/archive-context';
import { IArchive } from '../interfaces/IArchive';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
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

    var dispatchedValues: any[] = [];

    beforeEach(() => {
      // is used in multiple ways
      // use this: ==> import * as AppContext from '../contexts/archive-context';
      useContextSpy = jest
        .spyOn(React, 'useContext')
        .mockImplementation(() => contextValues);

      // clean array
      dispatchedValues = [];

      const contextValues = {
        state: {
          isReadOnly: false,
          fileIndexItems: [{
            fileName: 'test.jpg',
            parentDirectory: '/'
          }, {
            fileName: 'test1.jpg',
            parentDirectory: '/'
          }]
        } as IArchive,
        dispatch: (value: any) => {
          dispatchedValues.push(value)
        },
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
      useContextSpy.mockClear();
    });

    it("isReadOnly: false", () => {

      const component = shallow(<ArchiveSidebarLabelEditAddOverwrite />);

      var formControl = component.find('.form-control');

      // there are 3 classes [title,info,description]
      formControl.forEach(element => {
        expect(element.props()["contentEditable"]).toBeTruthy();
      });

      // if there is no contentEditable it should fail
      expect(formControl.length).toBeGreaterThanOrEqual(3);

      act(() => {
        component.unmount();
      });
    });

    it('Should change value when onChange was called', () => {
      const component = mount(<ArchiveSidebarLabelEditAddOverwrite />);

      act(() => {
        // update component
        component.find('[data-name="tags"]').getDOMNode().textContent = "a";

        // now press a key
        component.find('[data-name="tags"]').simulate('input', { key: 'a' })
      });

      var className = component.find('.btn.btn--default').getDOMNode().className
      expect(className).toBe('btn btn--default');

      act(() => {
        component.unmount();
      });
    });

    it('click append', async () => {


      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [{
          fileName: 'test.jpg',
          parentDirectory: '/',
          tags: 'test1, test2',
          status: IExifStatus.Ok
        }] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(connectionDefault);
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const component = mount(<ArchiveSidebarLabelEditAddOverwrite />);

      act(() => {
        // update component + now press a key
        component.find('[data-name="tags"]').getDOMNode().textContent = "a";
        component.find('[data-name="tags"]').simulate('input', { key: 'a' });
      });

      expect(component.exists('.btn--default')).toBeTruthy();

      // need to await to contain dispatchedValues
      await act(async () => {
        await component.find('.btn.btn--default').simulate('click');
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith("/api/update", "append=true&collections=true&tags=a&f=%2F%2F%2Ftest.jpg");

      expect(dispatchedValues).toStrictEqual([{
        type: 'update',
        fileName: 'test.jpg',
        parentDirectory: '/',
        tags: 'test1, test2',
        select: ['test.jpg'],
        status: IExifStatus.Ok
      }])

      act(() => {
        component.unmount();
      });

    });

    it('click append multiple', async () => {

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test.jpg,test1.jpg,notfound.jpg");
      });

      jest.spyOn(FetchPost, 'default').mockReset();

      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [
          {
            fileName: 'test.jpg',
            parentDirectory: '/',
            tags: 'test1, test2',
            status: IExifStatus.Ok
          },
          {
            fileName: 'test1.jpg',
            parentDirectory: '/',
            tags: 'test, test2',
            status: IExifStatus.Ok
          }
        ] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(connectionDefault);
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const component = mount(<ArchiveSidebarLabelEditAddOverwrite />);

      act(() => {
        // update component + now press a key
        component.find('[data-name="tags"]').getDOMNode().textContent = "a";
        component.find('[data-name="tags"]').simulate('input', { key: 'a' });
      });

      // need to await to contain dispatchedValues
      await act(async () => {
        await component.find('.btn.btn--default').simulate('click');
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith("/api/update", "append=true&collections=true&tags=a&f=%2F%2F%2Ftest.jpg%3B%2F%2F%2Ftest1.jpg");

      expect(dispatchedValues).toStrictEqual([{
        type: 'update',
        fileName: 'test.jpg',
        parentDirectory: '/',
        tags: 'test1, test2',
        select: ['test.jpg'],
        status: IExifStatus.Ok
      },
      {
        type: 'update',
        fileName: 'test1.jpg',
        parentDirectory: '/',
        tags: 'test, test2',
        select: ['test1.jpg'],
        status: IExifStatus.Ok
      }])
    });
  });
});
