import { storiesOf } from "@storybook/react";
import React from "react";
import { DetailViewContext } from '../../../contexts/detailview-context';
import { IRelativeObjects, PageType } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import DetailViewSidebar from './detail-view-sidebar';

storiesOf("components/organisms/detail-view-sidebar", module)
  .add("default", () => {
    var contextProvider = {
      dispatch: () => { },
      state: {
        breadcrumb: [],
        fileIndexItem: {
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
        relativeObjects: {} as IRelativeObjects,
        subPath: "/",
        status: IExifStatus.Default,
        pageType: PageType.DetailView,
        colorClassActiveList: [],
      } as any
    };
    return <DetailViewContext.Provider value={contextProvider}>
      <DetailViewSidebar status={IExifStatus.Default} filePath={"/t"}>></DetailViewSidebar>
    </DetailViewContext.Provider>
  })