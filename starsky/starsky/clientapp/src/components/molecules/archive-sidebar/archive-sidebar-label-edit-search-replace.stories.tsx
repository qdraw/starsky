import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebarLabelEditSearchReplace from "./archive-sidebar-label-edit-search-replace";
export default {
  title: "components/molecules/archive-sidebar/label-edit-search-replace"
};

export const Disabled = () => {
  Router.navigate("/");
  return <ArchiveSidebarLabelEditSearchReplace />;
};

Disabled.storyName = "disabled";

export const Enabled = () => {
  Router.navigate("/?select=test.jpg");
  const archive = {} as IArchiveProps;
  return (
    <ArchiveContextProvider {...archive}>
      {" "}
      <ArchiveSidebarLabelEditSearchReplace />
    </ArchiveContextProvider>
  );
};

Enabled.storyName = "enabled";
