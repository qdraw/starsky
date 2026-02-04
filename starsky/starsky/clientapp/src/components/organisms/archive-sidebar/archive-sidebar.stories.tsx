import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { PageType } from "../../../interfaces/IDetailView";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebar from "./archive-sidebar";
export default {
  title: "components/organisms/archive-sidebar"
};

export const Disabled = () => {
  Router.navigate("/?sidebar=true");
  return (
    <ArchiveSidebar
      pageType={PageType.Archive}
      subPath={"/"}
      isReadOnly={true}
      colorClassUsage={[]}
      fileIndexItems={newIFileIndexItemArray()}
    />
  );
};

Disabled.storyName = "disabled";

export const OneItemSelected = () => {
  Router.navigate("/?sidebar=true&select=test.jpg");
  const archive = {
    isReadOnly: false,
    fileIndexItems: [{ fileName: "test.jpg", filePath: "/test.jpg" }]
  } as IArchiveProps;
  return (
    <ArchiveSidebar
      pageType={PageType.Archive}
      subPath={"/"}
      isReadOnly={false}
      colorClassUsage={[]}
      fileIndexItems={archive.fileIndexItems}
    />
  );
};

OneItemSelected.storyName = "one item selected";
