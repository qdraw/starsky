import {
    IFileIndexItem,
    newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import ItemTextListView from "./item-text-list-view";

export default {
  title: "components/molecules/item-text-list-view"
};

export const Default = () => {
  return (
    <ItemTextListView
      fileIndexItems={newIFileIndexItemArray()}
      callback={() => {}}
    />
  );
};

Default.story = {
  name: "default"
};

export const _2Items = () => {
  const exampleData = [
    { fileName: "test.jpg", filePath: "/test.jpg" },
    { fileName: "test2.jpg", filePath: "/test2.jpg" }
  ] as IFileIndexItem[];
  return <ItemTextListView fileIndexItems={exampleData} />;
};

_2Items.story = {
  name: "2 items"
};
