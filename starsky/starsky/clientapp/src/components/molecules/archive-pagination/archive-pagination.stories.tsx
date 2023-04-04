import React from "react";
import { newIRelativeObjects } from "../../../interfaces/IDetailView";
import ArchivePagination from "./archive-pagination";

export default {
  title: "components/molecules/archive-pagination"
};

export const Default = () => {
  return <ArchivePagination relativeObjects={newIRelativeObjects()} />;
};

Default.story = {
  name: "default"
};
