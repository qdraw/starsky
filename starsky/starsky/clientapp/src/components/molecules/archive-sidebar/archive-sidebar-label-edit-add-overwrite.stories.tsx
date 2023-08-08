import { globalHistory } from "@reach/router";
import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import ArchiveSidebarLabelEditAddOverwrite from "./archive-sidebar-label-edit-add-overwrite";

export default {
  title: "components/molecules/archive-sidebar/label-edit-add-overwrite"
};

export const Disabled = () => {
  globalHistory.navigate("/");
  return <ArchiveSidebarLabelEditAddOverwrite />;
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
      <ArchiveSidebarLabelEditAddOverwrite />
    </ArchiveContextProvider>
  );
};

Enabled.story = {
  name: "enabled"
};
