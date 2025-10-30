import { MemoryRouter } from "react-router-dom";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ItemListView from "./item-list-view";
const exampleData8Selected = [
  { fileName: "test.jpg", filePath: "/test.jpg", lastEdited: "1" },
  { fileName: "test2.jpg", filePath: "/test2.jpg", lastEdited: "1" },
  { fileName: "test3.jpg", filePath: "/test3.jpg", lastEdited: "1" },
  { fileName: "test4.jpg", filePath: "/test4.jpg", lastEdited: "1" },
  { fileName: "test5.jpg", filePath: "/test5.jpg", lastEdited: "1" },
  { fileName: "test6.jpg", filePath: "/test6.jpg", lastEdited: "1" },
  { fileName: "test7.jpg", filePath: "/test7.jpg", lastEdited: "1" },
  { fileName: "test8.jpg", filePath: "/test8.jpg", lastEdited: "1" }
] as IFileIndexItem[];

export default {
  title: "components/molecules/item-list-view"
};

export const Default = () => {
  Router.navigate("/");
  return (
    <ItemListView iconList={true} fileIndexItems={newIFileIndexItemArray()} colorClassUsage={[]} />
  );
};

Default.storyName = "default";

export const HomeNoContent = () => {
  Router.navigate("/");
  return <ItemListView subPath="/" iconList={true} fileIndexItems={[]} colorClassUsage={[]} />;
};

HomeNoContent.storyName = "home no content";

export const _8ItemsSelectionDisabled = () => {
  Router.navigate("/");
  return (
    <MemoryRouter>
      <ItemListView iconList={true} fileIndexItems={exampleData8Selected} colorClassUsage={[]} />
    </MemoryRouter>
  );
};

_8ItemsSelectionDisabled.storyName = "8 items (selection disabled)";

export const _8ItemsSelectionEnabled = () => {
  Router.navigate("/?select=");
  return (
    <MemoryRouter>
      <ItemListView iconList={true} fileIndexItems={exampleData8Selected} colorClassUsage={[]} />
    </MemoryRouter>
  );
};

_8ItemsSelectionEnabled.storyName = "8 items (selection enabled)";

export const _8ItemsIconListFalseSelectionEnabled = () => {
  Router.navigate("/?select=");
  return (
    <MemoryRouter>
      <ItemListView iconList={false} fileIndexItems={exampleData8Selected} colorClassUsage={[]} />
    </MemoryRouter>
  );
};

_8ItemsIconListFalseSelectionEnabled.storyName = "8 items iconlist false (selection enabled)";
