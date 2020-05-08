import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import ListImageBox from './list-image-box';

storiesOf("components/molecules/list-image-box", module)
  .add("default", () => {
    var fileIndexItem = {
      fileName: 'test.jpg',
      colorClass: 1
    } as IFileIndexItem;
    return <ListImageBox item={fileIndexItem} />
  })
