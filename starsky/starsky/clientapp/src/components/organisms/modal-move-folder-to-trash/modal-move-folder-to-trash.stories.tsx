import { storiesOf } from "@storybook/react";
import ModalMoveFolderToTrash from "./modal-move-folder-to-trash";

storiesOf(
  "components/organisms/modal-move-folder-to-trash.stories",
  module
).add("default", () => {
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
});
