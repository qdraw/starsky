import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { PageType } from '../../../interfaces/IDetailView';
import { IFileIndexItem, newIFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';

storiesOf("components/molecules/archive-sidebar/color-class", module)
  .add("no items disabled", () => {
    globalHistory.navigate("/");
    return <ArchiveSidebarColorClass pageType={PageType.Archive} isReadOnly={false} fileIndexItems={newIFileIndexItemArray()} />
  })
  .add("enabled", () => {
    globalHistory.navigate("/?select=test.jpg");
    return <ArchiveSidebarColorClass pageType={PageType.Archive} isReadOnly={false} fileIndexItems={[newIFileIndexItem()] as IFileIndexItem[]} />
  })