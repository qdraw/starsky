import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalArchiveMkdir from "../../organisms/modal-archive-mkdir/modal-archive-mkdir";

interface IMenuOptionMkdirProps {
  readOnly: boolean;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

export const MenuOptionMkdir: React.FunctionComponent<IMenuOptionMkdirProps> = ({
  readOnly,
  state,
  dispatch
}) => {
  const [isModalMkdirOpen, setIsModalMkdirOpen] = useState(false);

  return (
    <>
      {/* Modal new directory */}
      {isModalMkdirOpen && !readOnly ? (
        <ModalArchiveMkdir
          state={state}
          dispatch={dispatch}
          handleExit={() => setIsModalMkdirOpen(!isModalMkdirOpen)}
          isOpen={isModalMkdirOpen}
        />
      ) : null}

      <MenuOptionModal
        isReadOnly={readOnly}
        isSet={isModalMkdirOpen}
        set={() => setIsModalMkdirOpen(!isModalMkdirOpen)}
        localization={localization.MessageMkdir}
        testName="mkdir"
      />
    </>
  );
};
