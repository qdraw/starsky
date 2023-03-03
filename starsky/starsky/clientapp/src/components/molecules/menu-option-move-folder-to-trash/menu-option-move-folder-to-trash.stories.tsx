import { storiesOf } from "@storybook/react";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionMoveFolderToTrash from "./menu-option-move-folder-to-trash";

storiesOf("components/molecules/menu-option-move-folder-to-trash", module).add(
  "default",
  () => {
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
  }
);
