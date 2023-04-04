import React from "react";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import ListImage from "./list-image";

export default {
  title: "components/atoms/list-image"
};

export const Default = () => {
  return (
    <ListImage alt={"alt"} fileHash={"src"} imageFormat={ImageFormat.jpg} />
  );
};

Default.story = {
  name: "default"
};
