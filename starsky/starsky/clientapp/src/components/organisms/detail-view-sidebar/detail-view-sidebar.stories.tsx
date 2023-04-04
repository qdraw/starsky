import { DetailViewContext } from "../../../contexts/detailview-context";
import { IRelativeObjects, PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import DetailViewSidebar from "./detail-view-sidebar";

export default {
  title: "components/organisms/detail-view-sidebar"
};

export const Default = () => {
  const contextProvider = {
    dispatch: () => {},
    state: {
      breadcrumb: [],
      fileIndexItem: {
        filePath: "/test.jpg",
        tags: "tags!",
        description: "description!",
        title: "title!",
        colorClass: 3,
        dateTime: "2019-09-15T17:29:59",
        lastEdited: new Date().toISOString(),
        make: "apple",
        model: "iPhone",
        aperture: 2,
        focalLength: 10,
        longitude: 1,
        latitude: 1,
        isoSpeed: 100
      } as IFileIndexItem,
      relativeObjects: {} as IRelativeObjects,
      subPath: "/test.jpg",
      status: IExifStatus.Default,
      pageType: PageType.DetailView,
      colorClassActiveList: []
    } as any
  };
  return (
    <DetailViewContext.Provider value={contextProvider}>
      <DetailViewSidebar
        state={contextProvider.state}
        dispatch={contextProvider.dispatch}
        status={IExifStatus.Default}
        filePath={"/test.jpg"}
      ></DetailViewSidebar>
    </DetailViewContext.Provider>
  );
};

Default.story = {
  name: "default"
};
