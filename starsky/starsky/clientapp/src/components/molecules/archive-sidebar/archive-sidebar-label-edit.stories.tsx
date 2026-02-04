import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebarLabelEdit from "./archive-sidebar-label-edit";
export default {
  title: "components/molecules/archive-sidebar/label-edit"
};

export const Disabled = () => {
  Router.navigate("/");
  return <ArchiveSidebarLabelEdit />;
};

Disabled.storyName = "disabled";

export const Enabled = () => {
  Router.navigate("/?select=test.jpg");
  const archive = {} as IArchiveProps;
  return (
    <ArchiveContextProvider {...archive}>
      {" "}
      <ArchiveSidebarLabelEdit />
    </ArchiveContextProvider>
  );
};

Enabled.storyName = "enabled";
