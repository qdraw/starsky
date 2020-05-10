import { storiesOf } from "@storybook/react";
import React from "react";
import { IFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ItemListView from './item-list-view';

storiesOf("components/molecules/item-list-view", module)
  .add("default", () => {
    return <ItemListView fileIndexItems={newIFileIndexItemArray()} colorClassUsage={[]} />
  })
  .add("8 items", () => {
    var exampleData = [
      { fileName: 'test.jpg', filePath: '/test.jpg' },
      { fileName: 'test2.jpg', filePath: '/test2.jpg' },
      { fileName: 'test3.jpg', filePath: '/test3.jpg' },
      { fileName: 'test4.jpg', filePath: '/test4.jpg' },
      { fileName: 'test2.jpg', filePath: '/test5.jpg' },
      { fileName: 'test6.jpg', filePath: '/test6.jpg' },
      { fileName: 'test7.jpg', filePath: '/test7.jpg' },
      { fileName: 'test8.jpg', filePath: '/test8.jpg' }
    ] as IFileIndexItem[]
    return <ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />
  })