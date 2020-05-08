import { storiesOf } from "@storybook/react";
import React from "react";
import ModalDownload from './modal-download';

storiesOf("components/organisms/modal-download", module)
  .add("default", () => {
    return <ModalDownload
      isOpen={true}
      collections={false}
      select={["/"]}
      handleExit={() => { }}>
      test
    </ModalDownload>
  })