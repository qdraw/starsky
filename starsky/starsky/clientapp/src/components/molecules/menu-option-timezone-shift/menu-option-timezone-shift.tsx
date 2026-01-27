import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalTimezoneShift from "../../organisms/modal-timezone-shift/modal-timezone-shift";

interface IMenuOptionTimezoneShiftProps {
  readOnly: boolean;
  select: string[];
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

export const MenuOptionTimezoneShift: React.FunctionComponent<IMenuOptionTimezoneShiftProps> = ({
  readOnly,
  select,
  state,
  dispatch
}) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Only show if files are selected
  const isVisible = select && select.length > 0;

  return (
    <>
      {isModalOpen && !readOnly && isVisible ? (
        <ModalTimezoneShift
          select={select}
          handleExit={() => {
            setIsModalOpen(false);
          }}
          isOpen={isModalOpen}
          dispatch={dispatch}
          state={state}
          historyLocationSearch={""}
          undoSelection={() => {}}
        />
      ) : null}

      <MenuOptionModal
        isReadOnly={readOnly || !isVisible}
        isSet={isModalOpen}
        set={() => setIsModalOpen(!isModalOpen)}
        localization={localization.MessageShiftPhotoTimestamps}
        testName="timezone-shift"
      />
    </>
  );
};
