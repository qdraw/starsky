import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { ArchiveContextProvider } from '../../../contexts/archive-context';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

storiesOf("components/molecules/archive-sidebar/selection-list", module)
  .add("disabled", () => {
    globalHistory.navigate("/");
    return <ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />
  })
  .add("one item selected", () => {
    globalHistory.navigate("/?select=test.jpg");
    var archive = {
      fileIndexItems: [{ fileName: 'test', filePath: '/test.jpg' }]
    } as IArchiveProps;
    return <ArchiveContextProvider {...archive}> <ArchiveSidebarSelectionList fileIndexItems={archive.fileIndexItems} /></ArchiveContextProvider>
  })