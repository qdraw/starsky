import { storiesOf } from "@storybook/react";
import React from "react";
import Modal from './modal';

storiesOf("components/atoms/modal", module)
  .add("default", () => {
    return <Modal
      id="rename-file-modal"
      isOpen={true}
      handleExit={() => { }}>
    </Modal>
  })