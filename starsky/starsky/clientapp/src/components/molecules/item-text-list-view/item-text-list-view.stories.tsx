import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ItemTextListView from "./item-text-list-view";

export default {
  title: "components/molecules/item-text-list-view"
};

export const Default = () => {
  return <ItemTextListView fileIndexItems={newIFileIndexItemArray()} callback={() => {}} />;
};

Default.storyName = "default";

export const _2Items = () => {
  const exampleData = [
    { fileName: "test.jpg", filePath: "/test.jpg" },
    { fileName: "test2.jpg", filePath: "/test2.jpg" },
    { fileName: "test3.jpg", filePath: "/test3.jpg", status: IExifStatus.ExifWriteNotSupported },
    { fileName: "server-error.jpg", filePath: "/server-error.jpg", status: IExifStatus.ServerError }
  ] as IFileIndexItem[];
  return <ItemTextListView fileIndexItems={exampleData} />;
};

_2Items.storyName = "2 items";
