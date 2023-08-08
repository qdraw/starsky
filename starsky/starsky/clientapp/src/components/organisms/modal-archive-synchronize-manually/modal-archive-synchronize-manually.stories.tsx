import ModalArchiveSynchronizeManually from "./modal-archive-synchronize-manually";

export default {
  title: "components/organisms/modal-archive-synchronize-manually"
};

export const Default = () => {
  return (
    <ModalArchiveSynchronizeManually isOpen={true} handleExit={() => {}} />
  );
};

Default.story = {
  name: "default"
};
