import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import MenuTrash from "./menu-trash";

export default {
  title: "components/organisms/menu-trash"
};

export const Default = () => {
  return (
    <MenuTrash state={{ fileIndexItems: [] } as unknown as IArchiveProps} dispatch={() => {}} />
  );
};

Default.storyName = "default";
