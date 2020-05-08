import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ItemListView from './item-list-view';

storiesOf("components/molecules/item-list-view", module)
  .add("default", () => {
    return <ItemListView fileIndexItems={newIFileIndexItemArray()} colorClassUsage={[]} />
  })
  .add("2 items", () => {
    var exampleData = [
      { fileName: 'test.jpg', filePath: '/test.jpg' },
      { fileName: 'test2.jpg', filePath: '/test2.jpg' }
    ] as IFileIndexItem[]
    return <ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />
  })