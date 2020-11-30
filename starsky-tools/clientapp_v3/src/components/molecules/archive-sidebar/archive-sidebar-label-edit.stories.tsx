import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { ArchiveContextProvider } from '../../../contexts/archive-context';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';

storiesOf("components/molecules/archive-sidebar/label-edit", module)
  .add("disabled", () => {
    globalHistory.navigate("/");
    return <ArchiveSidebarLabelEdit />
  })
  .add("enabled", () => {
    globalHistory.navigate("/?select=test.jpg");
    var archive = {} as IArchiveProps;
    return <ArchiveContextProvider {...archive}> <ArchiveSidebarLabelEdit /></ArchiveContextProvider>
  })