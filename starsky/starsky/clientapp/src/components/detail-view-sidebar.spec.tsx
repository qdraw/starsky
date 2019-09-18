import { mount, shallow } from 'enzyme';
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
            colorClass: 3
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

    });

    it("test if tags from the context is displayed", () => {
      const component = mount(<TestComponent />);
      var tags = component.find('[data-name="tags"]')

      expect(tags.text()).toBe('tags!')
    });

    it("test if title from the context is displayed", () => {
      const component = mount(<TestComponent />);
      var tags = component.find('[data-name="title"]')
      expect(tags.text()).toBe('title!')
    });

    it("test if description from the context is displayed", () => {
      const component = mount(<TestComponent />);
      var tags = component.find('[data-name="description"]')
      expect(tags.text()).toBe('description!')
    });

  });
});