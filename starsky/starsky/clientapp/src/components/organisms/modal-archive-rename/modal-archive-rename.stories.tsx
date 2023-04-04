import React from "react";
import ModalArchiveRename from "./modal-archive-rename";

export default {
  title: "components/organisms/modal-archive-rename"
};

export const Default = () => {
  return (
    <ModalArchiveRename
      subPath="/test/child_folder"
      isOpen={true}
      handleExit={() => {}}
    />
  );
};

Default.story = {
  name: "default"
};
