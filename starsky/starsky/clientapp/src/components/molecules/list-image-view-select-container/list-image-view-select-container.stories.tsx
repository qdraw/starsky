import { globalHistory } from "@reach/router";
import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ListImageBox from "./list-image-view-select-container";

const fileIndexItem = {
  fileName: "test.jpg",
  colorClass: 1
} as IFileIndexItem;

export default {
  title: "components/molecules/list-image-view-select-container"
};

export const Default = () => {
  globalHistory.navigate("/");
  return (
    <>
      <ListImageBox item={fileIndexItem}>
        <ListImageChildItem {...fileIndexItem} />
      </ListImageBox>
    </>
  );
  // for multiple items on page see: components/molecules/item-list-view
};

Default.story = {
  name: "default"
};

export const Select = () => {
  globalHistory.navigate("/?select=test.jpg");
  return (
    <>
      <ListImageBox item={fileIndexItem}>
        <ListImageChildItem {...fileIndexItem} />
      </ListImageBox>
    </>
  );
};

Select.story = {
  name: "select"
};
