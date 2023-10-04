import { PageType } from "../../../interfaces/IDetailView";
import {
  IFileIndexItem,
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ArchiveSidebarColorClass from "./archive-sidebar-color-class";
export default {
  title: "components/molecules/archive-sidebar/color-class"
};

export const NoItemsDisabled = () => {
  Router.navigate("/");
  return (
    <ArchiveSidebarColorClass
      pageType={PageType.Archive}
      isReadOnly={false}
      fileIndexItems={newIFileIndexItemArray()}
    />
  );
};

NoItemsDisabled.story = {
  name: "no items disabled"
};

export const Enabled = () => {
  Router.navigate("/?select=test.jpg");
  return (
    <ArchiveSidebarColorClass
      pageType={PageType.Archive}
      isReadOnly={false}
      fileIndexItems={[newIFileIndexItem()] as IFileIndexItem[]}
    />
  );
};

Enabled.story = {
  name: "enabled"
};
