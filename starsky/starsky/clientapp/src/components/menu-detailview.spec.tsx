import { mount, ReactWrapper, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { IDetailView } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import MenuDetailView from './menu-detailview';
import * as ModalDetailviewRenameFile from './modal-detailview-rename-file';
import * as ModalExport from './modal-export';

describe("MenuDetailView", () => {

  it("renders", () => {
    shallow(<MenuDetailView />)
  });

  describe("with Context", () => {

    let contextValues: any;
    let Component: ReactWrapper<any, Readonly<{}>>;

    beforeEach(() => {
      var state = {
        subPath: "/test/image.jpg",
        fileIndexItem: { status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView;
      contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      Component = mount(<MenuDetailView />)
    });

    it("export click", () => {
      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var exportModal = jest.spyOn(ModalExport, 'default')
        .mockImplementationOnce(() => { return <></> });

      var item = Component.find('[data-test="export"]');
      item.simulate('click');

      expect(exportModal).toBeCalled();
    });

    it("move click [Not implemented]", () => {
      // var moveModal = jest.spyOn(ModalExport, 'default')
      //   .mockImplementationOnce(() => { return <></> });

      // var item = Component.find('[data-test="move"]');
      // item.simulate('click');

      // expect(moveModal).toBeCalled();
    });

    it("rename click", () => {
      var renameModal = jest.spyOn(ModalDetailviewRenameFile, 'default')
        .mockImplementationOnce(() => { return <></> });

      var item = Component.find('[data-test="rename"]');
      item.simulate('click');

      expect(renameModal).toBeCalled();
    });

    it("trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var item = Component.find('[data-test="trash"]');
      item.simulate('click');

      expect(spy).toBeCalled();
      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith(new UrlQuery().UrlQueryUpdateApi(), "f=%2Ftest%2Fimage.jpg&Tags=%21delete%21&append=true")
    });


  });

  describe("file is marked as deleted", () => {

    it("trash click to trash", () => {
      var state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: { status: IExifStatus.Deleted, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return contextValues })

      const component = mount(<MenuDetailView />)

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var spy = jest.spyOn(FetchPost, 'default')
        .mockImplementationOnce(() => mockIConnectionDefault);

      var item = component.find('[data-test="trash"]');

      item.simulate('click');

      expect(spy).toBeCalledWith(new UrlQuery().UrlReplaceApi(),
        "f=%2Ftrashed%2Ftest1.jpg&fieldName=tags&search=%21delete%21");

      // for some reason the spy is called 2 times here?
    });

  });
});