import { storiesOf } from "@storybook/react";
import React from "react";
import { ImageFormat } from '../../../interfaces/IFileIndexItem';
import ListImage from './list-image';

storiesOf("components/atoms/list-image", module)
  .add("default", () => {
    return <ListImage alt={'alt'} fileHash={'src'} imageFormat={ImageFormat.jpg} />
  })