import { useState } from "react";
import localization from "../../../localization/localization.json";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalTimezoneShift from "../../organisms/modal-timezone-shift/modal-timezone-shift";

interface IMenuOptionTimezoneShiftProps {
  readOnly: boolean;
  select: string[];
  collections?: boolean;
}

export const MenuOptionTimezoneShift: React.FunctionComponent<IMenuOptionTimezoneShiftProps> = ({
  readOnly,
  select,
  collections = true
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
          collections={collections}
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
