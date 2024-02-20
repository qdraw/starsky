import ModalArchiveMkdir from "./modal-archive-mkdir";

export default {
  title: "components/organisms/modal-archive-mkdir"
};

export const Default = () => {
  return (
    <ModalArchiveMkdir
      state={{} as any}
      dispatch={() => {}}
      isOpen={true}
      handleExit={() => {}}
    ></ModalArchiveMkdir>
  );
};

Default.storyName = "default";
