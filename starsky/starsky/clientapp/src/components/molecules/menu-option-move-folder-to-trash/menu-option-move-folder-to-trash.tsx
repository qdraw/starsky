import React, { memo, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalMoveFolderToTrash from "../../organisms/modal-move-folder-to-trash/modal-move-folder-to-trash";
interface IMenuOptionMoveToTrashProps {
  subPath: string;
  isReadOnly: boolean;
  dispatch: React.Dispatch<ArchiveAction>;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

const MenuOptionMoveFolderToTrash: React.FunctionComponent<IMenuOptionMoveToTrashProps> =
  memo(({ isReadOnly, subPath, setEnableMoreMenu }) => {
    const [modalMoveFolderToTrashOpen, setModalMoveFolderToTrashOpen] =
      useState(false);

    return (
      <>
        {/* Modal move folder to trash */}
        {modalMoveFolderToTrashOpen ? (
          <ModalMoveFolderToTrash
            handleExit={() => {
              setModalMoveFolderToTrashOpen(!modalMoveFolderToTrashOpen);
            }}
            subPath={subPath}
            setIsLoading={() => {}}
            isOpen={modalMoveFolderToTrashOpen}
          />
        ) : null}

        <MenuOptionModal
          isReadOnly={isReadOnly}
          testName="move-folder-to-trash"
          isSet={modalMoveFolderToTrashOpen}
          set={setModalMoveFolderToTrashOpen}
          setEnableMoreMenu={setEnableMoreMenu}
          localization={localization.MessageMoveCurrentFolderToTrash}
        />
      </>
    );
  });

export default MenuOptionMoveFolderToTrash;
