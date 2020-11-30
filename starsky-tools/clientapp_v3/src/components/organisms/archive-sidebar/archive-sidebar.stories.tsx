import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import { PageType } from '../../../interfaces/IDetailView';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ArchiveSidebar from './archive-sidebar';

storiesOf("components/organisms/archive-sidebar", module)
  .add("disabled", () => {
    globalHistory.navigate("/?sidebar=true");
    return <ArchiveSidebar pageType={PageType.Archive} subPath={"/"} isReadOnly={true} colorClassUsage={[]} fileIndexItems={newIFileIndexItemArray()} />
  })
  .add("one item selected", () => {
    globalHistory.navigate("/?sidebar=true&select=test.jpg");
    var archive = {
      isReadOnly: false,
      fileIndexItems: [{ fileName: 'test.jpg', filePath: '/test.jpg' }]
    } as IArchiveProps;
    return <ArchiveSidebar pageType={PageType.Archive} subPath={"/"} isReadOnly={false} colorClassUsage={[]} fileIndexItems={archive.fileIndexItems} />
  })