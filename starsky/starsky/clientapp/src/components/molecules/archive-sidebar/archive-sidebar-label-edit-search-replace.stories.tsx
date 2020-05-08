import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { ArchiveContextProvider } from '../../../contexts/archive-context';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import ArchiveSidebarLabelEditSearchReplace from './archive-sidebar-label-edit-search-replace';

storiesOf("components/molecules/archive-sidebar/label-edit-search-replace", module)
  .add("disabled", () => {
    globalHistory.navigate("/");
    return <ArchiveSidebarLabelEditSearchReplace />
  })
  .add("enabled", () => {
    globalHistory.navigate("/?select=test.jpg");
    var archive = {} as IArchiveProps;
    return <ArchiveContextProvider {...archive}> <ArchiveSidebarLabelEditSearchReplace /></ArchiveContextProvider>
  })