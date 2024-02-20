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
    { fileName: "test2.jpg", filePath: "/test2.jpg" }
  ] as IFileIndexItem[];
  return <ItemTextListView fileIndexItems={exampleData} />;
};

_2Items.storyName = "2 items";
