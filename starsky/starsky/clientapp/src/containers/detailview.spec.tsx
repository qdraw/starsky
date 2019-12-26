import { globalHistory } from '@reach/router';
import { mount, ReactWrapper, shallow } from "enzyme";
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as ContextDetailview from '../contexts/detailview-context';
import * as useLocation from '../hooks/use-location';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IDetailView, IRelativeObjects, newDetailView, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem, Orientation } from '../interfaces/IFileIndexItem';
import * as FetchGet from '../shared/fetch-get';
import { UrlQuery } from '../shared/url-query';
import DetailView from './detailview';

describe("DetailView", () => {
  it("renders", () => {
    shallow(<DetailView {...newDetailView()} />)
  });

  var defaultState = {
    breadcrumb: [],
    fileIndexItem: {
      fileHash: 'hash',
      tags: 'tags!',
      description: 'description!',
      title: 'title!',
      colorClass: 3,
      dateTime: '2019-09-15T17:29:59',
      lastEdited: new Date().toISOString(),
      make: 'apple',
      model: 'iPhone',
      aperture: 2,
      focalLength: 10,
      longitude: 1,
      latitude: 1,
      orientation: Orientation.Horizontal,
      fileName: 'test.jpg',
      filePath: '/parentDirectory/test.jpg',
      parentDirectory: '/parentDirectory'
    } as IFileIndexItem,
    relativeObjects: { nextFilePath: 'next', prevFilePath: 'prev' } as IRelativeObjects,
    status: IExifStatus.Default,
    pageType: PageType.DetailView,
    colorClassFilterList: [],
    subPath: '/parentDirectory/test.jpg',
  } as IDetailView;

  describe("With context and test if image is loaded", () => {
    let contextProvider: any;
    let TestComponent: () => JSX.Element;
    let Component: ReactWrapper<any, Readonly<{}>>;

    // Setup mock
    beforeEach(() => {
      contextProvider = {
        dispatch: () => jest.fn(),
        state: defaultState
      };

      act(() => {
        TestComponent = () => (
          <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
            <DetailView {...newDetailView()} />
          </ContextDetailview.DetailViewContext.Provider>
        );
      });

      // Show extra information
      globalHistory.navigate("/?details=true");

      Component = mount(<TestComponent />);
    });

    afterAll(() => {
      Component = mount(<></>);
      TestComponent = () => (<></>);
    });

    it("test if image is loaded", () => {
      var image = Component.find('.image--default');
      act(() => {
        image.simulate('load');
      });
      expect(image.props().src).toBe(
        new UrlQuery().UrlThumbnailImage(contextProvider.state.fileIndexItem.fileHash));
      expect(Component.exists('.main--error')).toBeFalsy();
    });

    it("test if image is failed", () => {
      var image = Component.find('.image--default');
      image.simulate('error');
      expect(Component.exists('.main--error')).toBeTruthy()
    });

    it("check if Details exist", () => {
      expect(Component.exists('.sidebar')).toBeTruthy()
    });


  });

  describe("Nexts/Prev clicks ++ Rotation check", () => {
    let TestComponent: () => JSX.Element;

    beforeAll(() => {
      globalHistory.navigate("/?details=true");
    });

    // // Setup mock
    beforeEach(() => {
      defaultState.fileIndexItem.orientation = Orientation.Rotate270Cw;
      const contextProvider = {
        dispatch: () => jest.fn(),
        state: defaultState
      };

      TestComponent = () => (
        <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
          <DetailView {...newDetailView()} />
        </ContextDetailview.DetailViewContext.Provider>
      );

    });

    it("Rotation API is called return 202", () => {

      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 202, data: {
          subPath: "/test/image.jpg",
          pageType: 'DetailView',
          fileIndexItem: { orientation: Orientation.Rotate270Cw, fileHash: 'needed', status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
        } as IDetailView
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      mount(<TestComponent />);

      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));
    });

    it("Rotation API is called return 200", () => {

      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      mount(<TestComponent />);

      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));
    });

    it("Next Click", () => {
      var navigateSpy = jest.fn();
      var locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      });

      var detailview = mount(<TestComponent />);

      detailview.find(".nextprev--next").simulate('click');
      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=next", { "replace": true });
    });

    it("Prev Click", () => {
      const navigateSpy = jest.fn();
      const locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      });

      const detailview = mount(<TestComponent />);

      detailview.find(".nextprev--prev").simulate('click');
      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=prev", { "replace": true });
    });

    it("Prev Keyboard", () => {
      var navigateSpy = jest.fn();
      var locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      });

      mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowLeft",
        shiftKey: true,
      });
      window.dispatchEvent(event);

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=prev", { "replace": true });
    });

    it("Next Keyboard", () => {
      var navigateSpy = jest.fn();
      var locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy,
        }
      });

      mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowRight",
        shiftKey: true,
      });
      window.dispatchEvent(event);

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=next", { "replace": true });
    });

    it("Escape key Keyboard", () => {
      var navigateSpy = jest.fn();
      var locationSpy = jest.spyOn(useLocation, 'default').mockImplementationOnce(() => {
        return {
          location: { ...globalHistory.location, search: "" },
          navigate: navigateSpy,
        }
      });

      mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "Escape",
        shiftKey: true,
      });
      window.dispatchEvent(event);

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toHaveBeenNthCalledWith(1, "/?f=/parentDirectory", { "state": { "filePath": "/parentDirectory/test.jpg" } });
    });

  });
});