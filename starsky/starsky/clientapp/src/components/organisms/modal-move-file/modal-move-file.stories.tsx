import React from "react";
import ModalMoveFile from "./modal-move-file";

export default {
  title: "components/organisms/modal-move-file"
};

export const Default = () => {
  return (
    <ModalMoveFile
      parentDirectory="/"
      selectedSubPath="/test.jpg"
      isOpen={true}
      handleExit={() => {}}
    ></ModalMoveFile>
  );
};

Default.story = {
  name: "default"
};
