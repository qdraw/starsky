import { IDetailView } from "../../../interfaces/IDetailView";
import ModalDetailviewRenameFile from "./modal-detailview-rename-file";

export default {
  title: "components/organisms/modal-detailview-rename-file"
};

export const Default = () => {
  return (
    <ModalDetailviewRenameFile
      state={{} as IDetailView}
      isOpen={true}
      handleExit={() => {}}
    ></ModalDetailviewRenameFile>
  );
};

Default.storyName = "default";
