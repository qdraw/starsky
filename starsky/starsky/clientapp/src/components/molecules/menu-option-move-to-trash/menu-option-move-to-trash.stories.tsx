import { storiesOf } from "@storybook/react";
import React from "react";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

storiesOf("components/molecules/menu-option-move-to-trash", module).add(
  "default",
  () => {
    var test = {
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
        <MoreMenu defaultEnableMenu={true}>
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
