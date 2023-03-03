import React, { memo, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import localization from "../../../localization/localization.json";
import MenuOption from "../../atoms/menu-option/menu-option";
import ModalMoveFolderToTrash from "../../organisms/modal-move-folder-to-trash/modal-move-folder-to-trash";
interface IMenuOptionMoveToTrashProps {
  subPath: string;
  isReadOnly: boolean;
  dispatch: React.Dispatch<ArchiveAction>;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

const MenuOptionMoveFolderToTrash: React.FunctionComponent<IMenuOptionMoveToTrashProps> =
  memo(({ isReadOnly, subPath, setEnableMoreMenu }) => {
    const [isModalMoveFolderToTrashOpen, setModalMoveFolderToTrashOpen] =
      useState(false);

    return (
      <>
        {/* Modal move folder to trash */}
        {isModalMoveFolderToTrashOpen ? (
          <ModalMoveFolderToTrash
            handleExit={() => {
              setModalMoveFolderToTrashOpen(!isModalMoveFolderToTrashOpen);
            }}
            subPath={subPath}
            setIsLoading={() => {}}
            isOpen={isModalMoveFolderToTrashOpen}
          />
        ) : null}

        <MenuOption
          isReadOnly={isReadOnly}
          testName="move-folder-to-trash"
          isSet={isModalMoveFolderToTrashOpen}
          set={setModalMoveFolderToTrashOpen}
          setEnableMoreMenu={setEnableMoreMenu}
          localization={localization.MessageMoveCurrentFolderToTrash}
        />
      </>
    );
  });

export default MenuOptionMoveFolderToTrash;
