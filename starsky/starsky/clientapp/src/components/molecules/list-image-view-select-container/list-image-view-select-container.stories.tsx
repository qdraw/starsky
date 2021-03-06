import { globalHistory } from "@reach/router";
import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ListImageBox from "./list-image-view-select-container";

var fileIndexItem = {
  fileName: "test.jpg",
  colorClass: 1
} as IFileIndexItem;

storiesOf("components/molecules/list-image-view-select-container", module)
  .add("default", () => {
    globalHistory.navigate("/");
    return (
      <>
        <ListImageBox item={fileIndexItem}>
          <ListImageChildItem {...fileIndexItem} />
        </ListImageBox>
      </>
    );
    // for multiple items on page see: components/molecules/item-list-view
  })
  .add("select", () => {
    globalHistory.navigate("/?select=test.jpg");
    return (
      <>
        <ListImageBox item={fileIndexItem}>
          <ListImageChildItem {...fileIndexItem} />
        </ListImageBox>
      </>
    );
  });
