import { globalHistory } from "@reach/router";
import React from "react";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
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
  globalHistory.navigate("/");
  return (
    <ItemListView
      iconList={true}
      fileIndexItems={newIFileIndexItemArray()}
      colorClassUsage={[]}
    />
  );
};

Default.story = {
  name: "default"
};

export const _8ItemsSelectionDisabled = () => {
  globalHistory.navigate("/");
  return (
    <ItemListView
      iconList={true}
      fileIndexItems={exampleData8Selected}
      colorClassUsage={[]}
    />
  );
};

_8ItemsSelectionDisabled.story = {
  name: "8 items (selection disabled)"
};

export const _8ItemsSelectionEnabled = () => {
  globalHistory.navigate("/?select=");
  return (
    <ItemListView
      iconList={true}
      fileIndexItems={exampleData8Selected}
      colorClassUsage={[]}
    />
  );
};

_8ItemsSelectionEnabled.story = {
  name: "8 items (selection enabled)"
};
