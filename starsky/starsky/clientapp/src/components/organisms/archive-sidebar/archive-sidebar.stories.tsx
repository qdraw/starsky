import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { PageType } from "../../../interfaces/IDetailView";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ArchiveSidebar from "./archive-sidebar";
export default {
  title: "components/organisms/archive-sidebar"
};

export const Disabled = () => {
  window.location.replace("/?sidebar=true");
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

Disabled.story = {
  name: "disabled"
};

export const OneItemSelected = () => {
  window.location.replace("/?sidebar=true&select=test.jpg");
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

OneItemSelected.story = {
  name: "one item selected"
};
