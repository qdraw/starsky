import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import FlatListItemBox from "./flat-list-item-box";

storiesOf("components/molecules/flat-list-item-box", module).add(
  "default",
  () => {
    const exampleItem = [
      { fileName: "test.jpg", filePath: "/test.jpg", lastEdited: "1" }
    ] as IFileIndexItem[];

    return <FlatListItemBox item={exampleItem[0]} />;
  }
);
