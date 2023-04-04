import ModalMoveFolderToTrash from "./modal-move-folder-to-trash";

export default {
  title: "components/organisms/modal-move-folder-to-trash.stories"
};

export const Default = () => {
  return (
    <ModalMoveFolderToTrash
      isOpen={true}
      subPath={"/path/to/folder"}
      handleExit={() => {
        console.log("handleExit");
      }}
      setIsLoading={() => {
        console.log("setIsLoading");
      }}
    />
  );
};

Default.story = {
  name: "default"
};
