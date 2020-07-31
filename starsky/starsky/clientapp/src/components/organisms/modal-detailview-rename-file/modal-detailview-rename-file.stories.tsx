import { storiesOf } from "@storybook/react";
import React from "react";
import ModalDetailviewRenameFile from './modal-detailview-rename-file';

storiesOf("components/organisms/modal-detailview-rename-file", module)
  .add("default", () => {
    return <ModalDetailviewRenameFile
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalDetailviewRenameFile>
  })