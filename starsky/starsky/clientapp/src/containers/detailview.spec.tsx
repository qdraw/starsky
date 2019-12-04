import { globalHistory } from '@reach/router';
import { mount, ReactWrapper, shallow } from "enzyme";
import React from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import { IRelativeObjects, newDetailView } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { UrlQuery } from '../shared/url-query';
import DetailView from './detailview';

describe("DetailView", () => {
  it("renders", () => {
    shallow(<DetailView {...newDetailView()} />)
  });

  describe("With context", () => {
    let contextProvider: any;
    let TestComponent: () => JSX.Element;
    let Component: ReactWrapper<any, Readonly<{}>>;

    // Setup mock
    beforeEach(() => {
      contextProvider = {
        dispatch: () => jest.fn(),
        state: {
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
          } as IFileIndexItem,
          relativeObjects: { nextFilePath: 'next', prevFilePath: 'prev' } as IRelativeObjects,
          subPath: "/",
          status: IExifStatus.Default,
          pageType: 'DetailView',
          colorClassFilterList: [],
        } as any
      };

      TestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailView {...newDetailView()} />
        </DetailViewContext.Provider>
      );

      // Show extra information
      globalHistory.navigate("/?details=true");

      Component = mount(<TestComponent />);
    });

    it("test if image is loaded", () => {
      var image = Component.find('.image--default')
      image.simulate('load');

      expect(image.props().src).toBe(new UrlQuery().UrlQueryThumbnailImage(contextProvider.state.fileIndexItem.fileHash))
      expect(Component.exists('.main--error')).toBeFalsy();
    });

    it("test if image is failed", () => {
      var image = Component.find('.image--default')
      image.simulate('error');

      expect(Component.exists('.main--error')).toBeTruthy()
    });

    it("check if Details exist", () => {
      expect(Component.exists('.sidebar')).toBeTruthy()
    });

  });

});