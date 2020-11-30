import { storiesOf } from "@storybook/react";
import React from "react";
import { newIRelativeObjects } from '../../../interfaces/IDetailView';
import ArchivePagination from './archive-pagination';

storiesOf("components/molecules/archive-pagination", module)
  .add("default", () => {
    return <ArchivePagination relativeObjects={newIRelativeObjects()} />
  })