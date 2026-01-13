import { useState } from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalBatchRename from "../../organisms/modal-batch-rename/modal-batch-rename";

interface IMenuOptionBatchRenameProps {
  readOnly: boolean;
  state: IArchiveProps;
  selectedFilePaths: string[];
}

export const MenuOptionBatchRename: React.FunctionComponent<IMenuOptionBatchRenameProps> = ({
  readOnly,
  state,
  selectedFilePaths
}) => {
  const [isModalBatchRename, setIsModalBatchRename] = useState(false);

  // Only show if files are selected
  const isVisible = selectedFilePaths && selectedFilePaths.length > 0;

  return (
    <>
      {isModalBatchRename && !readOnly && isVisible ? (
        <ModalBatchRename
          selectedFilePaths={selectedFilePaths}
          handleExit={() => {
            setIsModalBatchRename(false);
          }}
          isOpen={isModalBatchRename}
        />
      ) : null}

      <MenuOptionModal
        isReadOnly={readOnly || !isVisible}
        isSet={isModalBatchRename}
        set={() => setIsModalBatchRename(!isModalBatchRename)}
        localization={localization.MessageBatchRenamePhotos}
        testName="batch-rename"
      />
    </>
  );
};
