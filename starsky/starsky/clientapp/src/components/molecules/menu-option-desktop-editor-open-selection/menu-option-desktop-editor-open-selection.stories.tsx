import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionDesktopEditorOpenSelection from "./menu-option-desktop-editor-open-selection";

export default {
  title: "components/molecules/menu-option-desktop-editor-open"
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
          } as any
        }
        isReadOnly={false}
        select={["default.jpg"]}
      />
    </MoreMenu>
  );
};

Default.storyName = "default (no dialog)";

export const Case2 = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSelection
        state={
          {
            fileIndexItems: [
              {
                fileName: "true.jpg",
                parentDirectory: "/"
              }
            ]
          } as any
        }
        isReadOnly={false}
        select={["true.jpg"]}
      />
    </MoreMenu>
  );
};

Case2.storyName = "with dialog";
