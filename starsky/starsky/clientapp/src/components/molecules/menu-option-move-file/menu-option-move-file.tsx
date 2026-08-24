import React, { memo } from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalMoveFile from "../../organisms/modal-move-file/modal-move-file";

interface IMenuOptionMoveFile {
  subPath: string | string[];
  parentDirectory: string;
  isReadOnly: boolean;
  selectedFileIndexItems?: IFileIndexItem[];
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

const MenuOptionMoveFile: React.FunctionComponent<IMenuOptionMoveFile> = memo(
  ({ isReadOnly, subPath, parentDirectory, selectedFileIndexItems, setEnableMoreMenu }) => {
    const [isModalMoveFile, setIsModalMoveFile] = React.useState(false);

    let selectedSubPath = "";
    if (typeof subPath === "string") {
      selectedSubPath = subPath;
    } else if (Array.isArray(subPath)) {
      for (const path of subPath) {
        selectedSubPath += `${path};`;
      }
    }

    return (
      <>
        {isModalMoveFile && !isReadOnly ? (
          <ModalMoveFile
            selectedSubPath={selectedSubPath}
            selectedFolderSubPaths={selectedFileIndexItems
              ?.filter((item) => item.isDirectory)
              .map((item) => item.filePath)}
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
