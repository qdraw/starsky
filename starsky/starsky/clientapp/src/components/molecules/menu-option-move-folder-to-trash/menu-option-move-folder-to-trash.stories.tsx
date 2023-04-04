import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionMoveFolderToTrash from "./menu-option-move-folder-to-trash";

export default {
  title: "components/molecules/menu-option-move-folder-to-trash"
};

export const Default = () => {
  return (
    <>
      <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
        <MenuOptionMoveFolderToTrash
          subPath="/test"
          isReadOnly={false}
          dispatch={() => {}}
        />
      </MoreMenu>
    </>
  );
};

Default.story = {
  name: "default"
};
