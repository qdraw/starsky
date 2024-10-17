import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalArchiveRename from "../../organisms/modal-archive-rename/modal-archive-rename";

interface IMenuOptionArchiveRenameProps {
  readOnly: boolean;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

export const MenuOptionArchiveRename: React.FunctionComponent<IMenuOptionArchiveRenameProps> = ({
  readOnly,
  state,
  dispatch
}) => {
  const [isModalRenameFolder, setIsModalRenameFolder] = useState(false);

  return (
    <>
      {isModalRenameFolder && !readOnly && state.subPath !== "/" ? (
        <ModalArchiveRename
          subPath={state.subPath}
          dispatch={dispatch}
          handleExit={() => {
            setIsModalRenameFolder(!isModalRenameFolder);
          }}
          isOpen={isModalRenameFolder}
        />
      ) : null}

      <MenuOptionModal
        isReadOnly={readOnly || state.subPath === "/"}
        isSet={isModalRenameFolder}
        set={() => setIsModalRenameFolder(!isModalRenameFolder)}
        localization={localization.MessageRenameDir}
        testName="rename"
      />
    </>
  );
};
