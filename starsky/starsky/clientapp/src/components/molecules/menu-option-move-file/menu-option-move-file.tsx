import React, { memo } from "react";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalMoveFile from "../../organisms/modal-move-file/modal-move-file";

interface IMenuOptionMoveFile {
  subPath: string | string[];
  parentDirectory: string;
  isReadOnly: boolean;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

const MenuOptionMoveFile: React.FunctionComponent<IMenuOptionMoveFile> = memo(
  ({ isReadOnly, subPath, parentDirectory, setEnableMoreMenu }) => {
    const [isModalMoveFile, setIsModalMoveFile] = React.useState(false);

    let selectedSubPath = "";
    if (typeof subPath === "string") {
      selectedSubPath = subPath;
    } else if (Array.isArray(subPath)) {
      for (let i = 0; i < subPath.length; i++) {
        selectedSubPath += `${subPath[i]};`;
      }
    }

    return (
      <>
        {isModalMoveFile && !isReadOnly ? (
          <ModalMoveFile
            selectedSubPath={selectedSubPath}
            parentDirectory={parentDirectory}
            handleExit={() => setIsModalMoveFile(!isModalMoveFile)}
            isOpen={isModalMoveFile}
          />
        ) : null}

        <MenuOptionModal
          isReadOnly={isReadOnly}
          isSet={isModalMoveFile}
          set={() => setIsModalMoveFile(!isModalMoveFile)}
          localization={localization.MessageMove}
          setEnableMoreMenu={setEnableMoreMenu}
          testName="move"
        />
      </>
    );
  }
);

export default MenuOptionMoveFile;
