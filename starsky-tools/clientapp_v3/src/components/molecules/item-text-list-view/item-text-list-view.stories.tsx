import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ItemTextListView from './item-text-list-view';

storiesOf("components/molecules/item-text-list-view", module)
  .add("default", () => {
    return <ItemTextListView fileIndexItems={newIFileIndexItemArray()} callback={() => { }} />
  })
  .add("2 items", () => {
    var exampleData = [
      { fileName: 'test.jpg', filePath: '/test.jpg' },
      { fileName: 'test2.jpg', filePath: '/test2.jpg' }
    ] as IFileIndexItem[]
    return <ItemTextListView fileIndexItems={exampleData} />
  })