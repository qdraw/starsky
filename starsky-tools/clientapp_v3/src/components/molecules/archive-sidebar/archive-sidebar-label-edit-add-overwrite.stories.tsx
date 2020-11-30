import { globalHistory } from '@reach/router';
import { storiesOf } from "@storybook/react";
import React from "react";
import { ArchiveContextProvider } from '../../../contexts/archive-context';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import ArchiveSidebarLabelEditAddOverwrite from './archive-sidebar-label-edit-add-overwrite';

storiesOf("components/molecules/archive-sidebar/label-edit-add-overwrite", module)
  .add("disabled", () => {
    globalHistory.navigate("/");
    return <ArchiveSidebarLabelEditAddOverwrite />
  })
  .add("enabled", () => {

    globalHistory.navigate("/?select=test.jpg");
    var archive = {} as IArchiveProps;
    return <ArchiveContextProvider {...archive}> <ArchiveSidebarLabelEditAddOverwrite /></ArchiveContextProvider>
  })