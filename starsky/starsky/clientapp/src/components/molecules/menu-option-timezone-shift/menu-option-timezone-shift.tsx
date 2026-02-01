import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import { Select } from "../../../shared/select";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalTimezoneShift from "../../organisms/modal-timezone-shift/modal-timezone-shift";

interface IMenuOptionTimezoneShiftProps {
  readOnly: boolean;
  select: string[];
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
  setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>;
}

export const MenuOptionTimezoneShift: React.FunctionComponent<IMenuOptionTimezoneShiftProps> = ({
  readOnly,
  select,
  state,
  dispatch,
  setSelect
}) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Only show if files are selected
  const isVisible = select && select.length > 0;
  const history = useLocation();
  const undoSelection = () => new Select(select, setSelect, state, history).undoSelection();

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
          historyLocationSearch={history.location.search}
          undoSelection={undoSelection}
        />
      ) : null}

      <MenuOptionModal
        isReadOnly={readOnly || !isVisible}
        isSet={isModalOpen}
        set={() => setIsModalOpen(!isModalOpen)}
        localization={localization.MessageShiftPhotoTime}
        testName="timezone-shift"
      />
    </>
  );
};
