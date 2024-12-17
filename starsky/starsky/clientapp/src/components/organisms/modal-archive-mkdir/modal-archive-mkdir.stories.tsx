import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import ModalArchiveMkdir from "./modal-archive-mkdir";

export default {
  title: "components/organisms/modal-archive-mkdir"
};

export const Default = () => {
  return (
    <ModalArchiveMkdir
      state={{} as IArchiveProps}
      dispatch={() => {}}
      isOpen={true}
      handleExit={() => {}}
    ></ModalArchiveMkdir>
  );
};

Default.storyName = "default";
