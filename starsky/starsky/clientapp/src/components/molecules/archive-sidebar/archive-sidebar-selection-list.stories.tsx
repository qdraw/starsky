import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebarSelectionList from "./archive-sidebar-selection-list";
export default {
  title: "components/molecules/archive-sidebar/selection-list"
};

export const Disabled = () => {
  Router.navigate("/");
  return (
    <ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />
  );
};

Disabled.story = {
  name: "disabled"
};

export const OneItemSelected = () => {
  Router.navigate("/?select=test.jpg");
  const archive = {
    fileIndexItems: [{ fileName: "test", filePath: "/test.jpg" }]
  } as IArchiveProps;
  return (
    <ArchiveContextProvider {...archive}>
      {" "}
      <ArchiveSidebarSelectionList fileIndexItems={archive.fileIndexItems} />
    </ArchiveContextProvider>
  );
};

OneItemSelected.story = {
  name: "one item selected"
};
