import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ModalDropAreaFilesAdded from "./modal-drop-area-files-added";

export default {
  title: "components/molecules/modal-drop-area-files-added"
};

export const Default = () => {
  return <ModalDropAreaFilesAdded isOpen={true} uploadFilesList={[]} handleExit={() => {}} />;
};

Default.story = {
  name: "default"
};

export const _2Items = () => {
  return (
    <ModalDropAreaFilesAdded
      isOpen={true}
      uploadFilesList={[
        { fileName: "test.jpg", status: IExifStatus.Ok } as IFileIndexItem,
        {
          fileName: "file-error.png",
          status: IExifStatus.FileError
        } as IFileIndexItem
      ]}
      handleExit={() => {}}
    />
  );
};

_2Items.story = {
  name: "2 items"
};
