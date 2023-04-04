import { globalHistory } from "@reach/router";
import React from "react";
import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import ArchiveSidebarLabelEditSearchReplace from "./archive-sidebar-label-edit-search-replace";

export default {
  title: "components/molecules/archive-sidebar/label-edit-search-replace"
};

export const Disabled = () => {
  globalHistory.navigate("/");
  return <ArchiveSidebarLabelEditSearchReplace />;
};

Disabled.story = {
  name: "disabled"
};

export const Enabled = () => {
  globalHistory.navigate("/?select=test.jpg");
  const archive = {} as IArchiveProps;
  return (
    <ArchiveContextProvider {...archive}>
      {" "}
      <ArchiveSidebarLabelEditSearchReplace />
    </ArchiveContextProvider>
  );
};

Enabled.story = {
  name: "enabled"
};
