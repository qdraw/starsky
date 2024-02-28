import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionDesktopEditorOpenSelection from "./menu-option-desktop-editor-open-selection";

export default {
  title: "components/molecules/menu-option-desktop-editor-open-selection"
};

export const Default = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSelection
        state={
          {
            fileIndexItems: [
              {
                fileName: "default.jpg",
                parentDirectory: "/"
              }
            ]
          } as unknown as IArchiveProps
        }
        isReadOnly={false}
        select={["default.jpg"]}
      />
    </MoreMenu>
  );
};

Default.storyName = "default (no dialog)";

export const WithDialog = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSelection
        state={
          {
            fileIndexItems: [
              {
                fileName: "true.jpg", // in mock is set that true.jpg gives a dialog
                parentDirectory: "/"
              }
            ]
          } as unknown as IArchiveProps
        }
        isReadOnly={false}
        select={["true.jpg"]}
      />
    </MoreMenu>
  );
};

WithDialog.storyName = "with dialog";

export const ReadOnly = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSelection
        state={
          {
            fileIndexItems: [
              {
                fileName: "default.jpg",
                parentDirectory: "/"
              }
            ]
          } as unknown as IArchiveProps
        }
        isReadOnly={true}
        select={["default.jpg"]}
      />
    </MoreMenu>
  );
};

ReadOnly.storyName = "ReadOnly";
