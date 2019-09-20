import { mount, ReactWrapper, shallow } from 'enzyme';
import React from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import { IRelativeObjects } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import DetailViewSidebar from './detail-view-sidebar';

describe("DetailViewSidebar", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewSidebar status={IExifStatus.Default}
      filePath={"/t"}>></DetailViewSidebar>)
  });

  it("test warning (without state component)", () => {
    var wrapper = shallow(<DetailViewSidebar status={IExifStatus.Default} filePath={"/t"}>></DetailViewSidebar>)
    expect(wrapper.find('.sidebar').find('.warning-box')).toHaveLength(1);
  });

  describe("useContext", () => {
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
            tags: 'tags!',
            description: 'description!',
            title: 'title!',
            colorClass: 3,
            dateTime: '2019-09-15T17:29:59',
            lastEdited: new Date().toISOString()
          } as IFileIndexItem,
          relativeObjects: {} as IRelativeObjects,
          subPath: "/",
          status: IExifStatus.Default,
          pageType: 'DetailView',
          colorClassFilterList: [],
        } as any
      };

      TestComponent = () => (
        <DetailViewContext.Provider value={contextProvider}>
          <DetailViewSidebar status={IExifStatus.Default} filePath={"/t"}>></DetailViewSidebar>
        </DetailViewContext.Provider>
      );
      Component = mount(<TestComponent />);
    });

    it("test if tags from the context is displayed", () => {
      var tags = Component.find('[data-name="tags"]')
      expect(tags.text()).toBe('tags!')
    });

    it("test if title from the context is displayed", () => {
      var title = Component.find('[data-name="title"]')
      expect(title.text()).toBe('title!')
    });

    it("test if description from the context is displayed", () => {
      var description = Component.find('[data-name="description"]')
      expect(description.text()).toBe('description!')
    });

    it("test if colorclass from the context is displayed", () => {
      expect(Component.exists('.colorclass--3.active')).toBeTruthy()
      // the rest is false
      expect(Component.exists('.colorclass--1.active')).toBeFalsy()
      expect(Component.exists('.colorclass--2.active')).toBeFalsy()
      expect(Component.exists('.colorclass--4.active')).toBeFalsy()
      expect(Component.exists('.colorclass--5.active')).toBeFalsy()
      expect(Component.exists('.colorclass--6.active')).toBeFalsy()
      expect(Component.exists('.colorclass--7.active')).toBeFalsy()
      expect(Component.exists('.colorclass--8.active')).toBeFalsy()
    });

    it("test if dateTime from the context is displayed", () => {
      var dateTime = Component.find('[data-test="dateTime"]')
      expect(dateTime.text()).toBe("15-9-201917:29:59")
    });

    it("test if lastEdited from the context is displayed", () => {
      var lastEdited = Component.find('[data-test="lastEdited"]')
      expect(lastEdited.text()).toBe("minder dan 1 minuutgeleden bewerkt")
    });

  });
});