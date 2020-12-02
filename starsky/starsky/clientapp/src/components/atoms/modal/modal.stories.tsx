import { storiesOf } from "@storybook/react";
import React from "react";
import Modal from "./modal";

storiesOf("components/atoms/modal", module).add("default", () => {
  return (
    <Modal id="test-modal" isOpen={true} handleExit={() => {}}>
      data
    </Modal>
  );
});
