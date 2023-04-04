import Modal from "./modal";

export default {
  title: "components/atoms/modal"
};

export const Default = () => {
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
};

Default.story = {
  name: "default"
};
