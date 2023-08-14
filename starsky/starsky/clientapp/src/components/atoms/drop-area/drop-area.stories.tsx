import { useState } from "react";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import ModalDropAreaFilesAdded from "../../molecules/modal-drop-area-files-added/modal-drop-area-files-added";
import DropArea from "./drop-area";

export default {
  title: "components/atoms/drop-area"
};

export const Default = () => {
  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = useState(
    newIFileIndexItemArray()
  );

  return (
    <>
      <div>
        Drag &apos;n drop file to show
        <br />
        <br />
      </div>
      <DropArea
        enableDragAndDrop={true}
        enableInputButton={true}
        callback={(add) => {
          setDropAreaUploadFilesList(add);
        }}
        endpoint="/starsky/api/import"
      />

      {/* Upload drop Area */}
      {dropAreaUploadFilesList.length !== 0 ? (
        <ModalDropAreaFilesAdded
          handleExit={() =>
            setDropAreaUploadFilesList(newIFileIndexItemArray())
          }
          uploadFilesList={dropAreaUploadFilesList}
          isOpen={dropAreaUploadFilesList.length !== 0}
        />
      ) : null}
    </>
  );
};

Default.story = {
  name: "default"
};
