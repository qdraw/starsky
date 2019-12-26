import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import * as FetchGet from '../shared/fetch-get';
import * as FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
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

    beforeEach(() => {
      var state = {
        subPath: "/test/image.jpg",
        fileIndexItem: {
          status: IExifStatus.Ok,
          fileHash: '000',
          filePath: "/test/image.jpg",
          fileName: "image.jpg"
        }
      } as IDetailView;
      contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
    });

    it("as search Result button exist", () => {
      // add search query to url
      globalHistory.navigate("/?t=test&p=0");

      var component = mount(<MenuDetailView />);

      expect(component.exists('.item--search')).toBeTruthy();

      // reset afterwards
      component.unmount();
      globalHistory.navigate("/");
    });

    it("export click [menu]", () => {
      // one extra spy
      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var exportModal = jest.spyOn(ModalExport, 'default')
        .mockImplementationOnce(() => { return <></> });

      var component = mount(<MenuDetailView />);

      var item = component.find('[data-test="export"]');
      act(() => {
        item.simulate('click');
      });
      expect(exportModal).toBeCalled();

      // to avoid polling afterwards
      component.unmount();

    });

    it("labels click .item--labels [menu]", () => {

      // one extra spy
      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuDetailView />);

      var find = component.find('.item.item--labels');
      act(() => {
        find.simulate('click');
      });
      var urlObject = new URLPath().StringToIUrl(globalHistory.location.search);

      expect(urlObject.details).toBeTruthy();

      // dont keep any menus open
      component.unmount();

      // reset afterwards
      globalHistory.navigate("/");
    });

    it("labels click (in MoreMenu)", () => {
      var item = mount(<MenuDetailView />).find('[data-test="labels"]');

      act(() => {
        item.simulate('click');
      });

      var urlObject = new URLPath().StringToIUrl(globalHistory.location.search);
      expect(urlObject.details).toBeTruthy();

      // reset afterwards
      globalHistory.navigate("/");
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

      // one extra spy
      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var item = mount(<MenuDetailView />).find('[data-test="rename"]');
      act(() => {
        item.simulate('click');
      });

      expect(renameModal).toBeCalled();
    });

    it("trash click to trash", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var item = mount(<MenuDetailView />).find('[data-test="trash"]');
      act(() => {
        item.simulate('click');
      });

      expect(spy).toBeCalled();
      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith(new UrlQuery().UrlUpdateApi(), "f=%2Ftest%2Fimage.jpg&Tags=%21delete%21&append=true")
    });

    it("rotate click", async () => {

      jest.useFakeTimers();
      jest.spyOn(global, 'setTimeout');

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var spyPost = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: {
          subPath: "/test/image.jpg",
          pageType: PageType.DetailView,
          fileIndexItem: { fileHash: 'needed', status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
        } as IDetailView
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      var item = mount(<MenuDetailView />).find('[data-test="rotate"]');

      // need to await here
      await item.simulate('click');

      jest.advanceTimersByTime(5000);

      expect(spyPost).toBeCalled();
      expect(spyPost).toBeCalledWith(new UrlQuery().UrlUpdateApi(), "f=%2Ftest%2Fimage.jpg&rotateClock=1");

      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledWith(new UrlQuery().UrlIndexServerApi({ f: "/test/image.jpg" }));

      jest.useRealTimers();
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
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);

      var spy = jest.spyOn(FetchPost, 'default')
        .mockImplementationOnce(() => mockIConnectionDefault);

      var item = component.find('[data-test="trash"]');

      item.simulate('click');

      expect(spy).toBeCalledWith(new UrlQuery().UrlReplaceApi(),
        "f=%2Ftrashed%2Ftest1.jpg&fieldName=tags&search=%21delete%21");

      // for some reason the spy is called 2 times here?
    });


    //  file is marked as deleted › press 'Delete' on keyboard to trash
    it("press 'Delete' on keyboard to trash", () => {
      jest.clearAllMocks();

      var state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: { status: IExifStatus.Deleted, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      mount(<MenuDetailView />)

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var event = new KeyboardEvent("keydown", {
        bubbles: false,
        cancelable: true,
        key: "Delete",
        shiftKey: false,
        repeat: false,
      });
      window.dispatchEvent(event);

      expect(spy).toBeCalled();
      expect(spy).toBeCalledWith(new UrlQuery().UrlUpdateApi(), "f=%2Ftest%2Fimage.jpg&Tags=%21delete%21&append=true");
      // in the test the 'keyboard event fired' three times, but in the real world once
    });

    it("navigate to next item and reset some states", () => {

      var state = {
        subPath: "/trashed/test1.jpg",
        fileIndexItem: { status: IExifStatus.Deleted, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })


      var exportModal = jest.spyOn(ModalExport, 'default')
        .mockImplementationOnce(() => { return <></> });

      const component = mount(<MenuDetailView />)

      globalHistory.navigate("/?f=/test2.jpg");


      expect(component.find('header').getDOMNode().className).toBe("header header--main")

      // globalHistory.navigate("/");

    });

  });
});