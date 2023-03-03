import { storiesOf } from "@storybook/react";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

storiesOf("components/molecules/menu-option-move-to-trash", module).add(
  "default",
  () => {
    const test = {
      ...newIArchive(),
      fileIndexItems: [
        {
          parentDirectory: "/",
          fileName: "test.jpg"
        } as IFileIndexItem
      ]
    } as IArchiveProps;
    return (
      <>
        <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
          <MenuOptionMoveToTrash
            setSelect={() => {}}
            select={["test.jpg"]}
            isReadOnly={false}
            state={test}
            dispatch={() => {}}
          />
        </MoreMenu>
      </>
    );
  }
);
