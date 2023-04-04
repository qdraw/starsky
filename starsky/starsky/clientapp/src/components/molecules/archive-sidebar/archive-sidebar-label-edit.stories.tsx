import { globalHistory } from "@reach/router";
import React from "react";
import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import ArchiveSidebarLabelEdit from "./archive-sidebar-label-edit";

export default {
  title: "components/molecules/archive-sidebar/label-edit"
};

export const Disabled = () => {
  globalHistory.navigate("/");
  return <ArchiveSidebarLabelEdit />;
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
      <ArchiveSidebarLabelEdit />
    </ArchiveContextProvider>
  );
};

Enabled.story = {
  name: "enabled"
};
