import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import ModalDesktopEditorOpenSelectionConfirmation from "./modal-desktop-editor-open-selection-confirmation";

export default {
  title: "components/organisms/modal-desktop-editor-open-selection-confirmation"
};

export const Default = () => {
  return (
    <ModalDesktopEditorOpenSelectionConfirmation
      state={
        {
          fileIndexItems: [
            {
              fileName: "test",
              parentDirectory: "/"
            }
          ]
        } as unknown as IArchiveProps
      }
      isCollections={true}
      select={[]}
      setIsLoading={() => {}}
      isOpen={true}
      handleExit={() => {}}
    />
  );
};

Default.storyName = "default";
