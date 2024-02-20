import ModalPublish from "./modal-publish";

export default {
  title: "components/organisms/modal-publish"
};

export const Default = () => {
  return <ModalPublish isOpen={true} select={["/"]} handleExit={() => {}}></ModalPublish>;
};

Default.storyName = "default";
