import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionDesktopEditorOpenSingle from "./menu-option-desktop-editor-open-single";

export default {
  title: "components/molecules/menu-option-desktop-editor-open-single"
};

export const Default = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSingle
        subPath="/test.jpg"
        collections={true}
        isReadOnly={false}
      />
    </MoreMenu>
  );
};

Default.storyName = "default (no dialog)";

export const ReadOnly = () => {
  return (
    <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
      <MenuOptionDesktopEditorOpenSingle subPath="/test.jpg" collections={true} isReadOnly={true} />
    </MoreMenu>
  );
};

ReadOnly.storyName = "readonly";
