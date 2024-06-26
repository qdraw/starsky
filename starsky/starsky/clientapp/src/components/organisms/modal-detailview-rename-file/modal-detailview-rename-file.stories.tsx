import ModalDetailviewRenameFile from "./modal-detailview-rename-file";

export default {
  title: "components/organisms/modal-detailview-rename-file"
};

export const Default = () => {
  return (
    <ModalDetailviewRenameFile
      state={{} as any}
      isOpen={true}
      handleExit={() => {}}
    ></ModalDetailviewRenameFile>
  );
};

Default.storyName = "default";
