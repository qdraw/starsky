import { storiesOf } from "@storybook/react";
import Modal from "./modal";

storiesOf("components/atoms/modal", module).add("default", () => {
  return (
    <Modal
      id="test-modal"
      isOpen={true}
      handleExit={() => {
        console.log("exit");
      }}
    >
      data
    </Modal>
  );
});
