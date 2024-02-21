import ModalDesktopEditorOpenConfirmation from "./modal-desktop-editor-open-confirmation";

export default {
  title: "components/organisms/modal-desktop-editor-open-confirmation"
};

export const Default = () => {
  return (
    <ModalDesktopEditorOpenConfirmation
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
