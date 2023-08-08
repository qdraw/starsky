import { globalHistory } from "@reach/router";
import { ArchiveContextProvider } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ArchiveSidebarSelectionList from "./archive-sidebar-selection-list";

export default {
  title: "components/molecules/archive-sidebar/selection-list"
};

export const Disabled = () => {
  globalHistory.navigate("/");
  return (
    <ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />
  );
};

Disabled.story = {
  name: "disabled"
};

export const OneItemSelected = () => {
  globalHistory.navigate("/?select=test.jpg");
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
