import { storiesOf } from "@storybook/react";
import React from "react";
import ModalArchiveSynchronizeManually from './modal-archive-synchronize-manually';

storiesOf("components/organisms/modal-archive-synchronize-manually", module)
  .add("default", () => {
    return <ModalArchiveSynchronizeManually
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalArchiveSynchronizeManually>
  })