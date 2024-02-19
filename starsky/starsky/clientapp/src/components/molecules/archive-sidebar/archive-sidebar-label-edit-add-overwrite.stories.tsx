import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebarLabelEditAddOverwrite from "./archive-sidebar-label-edit-add-overwrite";
export default {
  title: "components/molecules/archive-sidebar/label-edit-add-overwrite"
};

export const Disabled = () => {
  Router.navigate("/");
  return <ArchiveSidebarLabelEditAddOverwrite />;
};

Disabled.storyName = "disabled";

export const Enabled = () => {
  Router.navigate("/?select=test.jpg");
  const archive = {} as IArchiveProps;
  return (
    <ArchiveContextProvider {...archive}>
      {" "}
      <ArchiveSidebarLabelEditAddOverwrite />
    </ArchiveContextProvider>
  );
};

Enabled.storyName = "enabled";
