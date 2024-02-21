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
        } as any
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
