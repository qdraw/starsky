import { globalHistory } from "@reach/router";
import { storiesOf } from "@storybook/react";
import React from "react";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import ItemListView from "./item-list-view";

var exampleData8Selected = [
  { fileName: "test.jpg", filePath: "/test.jpg", lastEdited: "1" },
  { fileName: "test2.jpg", filePath: "/test2.jpg", lastEdited: "1" },
  { fileName: "test3.jpg", filePath: "/test3.jpg", lastEdited: "1" },
  { fileName: "test4.jpg", filePath: "/test4.jpg", lastEdited: "1" },
  { fileName: "test5.jpg", filePath: "/test5.jpg", lastEdited: "1" },
  { fileName: "test6.jpg", filePath: "/test6.jpg", lastEdited: "1" },
  { fileName: "test7.jpg", filePath: "/test7.jpg", lastEdited: "1" },
  { fileName: "test8.jpg", filePath: "/test8.jpg", lastEdited: "1" }
] as IFileIndexItem[];

storiesOf("components/molecules/item-list-view", module)
  .add("default", () => {
    globalHistory.navigate("/");
    return (
      <ItemListView
        iconList={true}
        fileIndexItems={newIFileIndexItemArray()}
        colorClassUsage={[]}
      />
    );
  })
  .add("8 items (selection disabled)", () => {
    globalHistory.navigate("/");
    return (
      <ItemListView
        iconList={true}
        fileIndexItems={exampleData8Selected}
        colorClassUsage={[]}
      />
    );
  })
  .add("8 items (selection enabled)", () => {
    globalHistory.navigate("/?select=");
    return (
      <ItemListView
        iconList={true}
        fileIndexItems={exampleData8Selected}
        colorClassUsage={[]}
      />
    );
  });
