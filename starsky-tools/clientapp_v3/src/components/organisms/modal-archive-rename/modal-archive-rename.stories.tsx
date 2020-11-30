import { storiesOf } from "@storybook/react";
import React from "react";
import ModalArchiveRename from './modal-archive-rename';

storiesOf("components/organisms/modal-archive-rename", module)
  .add("default", () => {
    return <ModalArchiveRename
      subPath="/test/child_folder"
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalArchiveRename>
  })