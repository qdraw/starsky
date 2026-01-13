import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import { Select } from "../../../shared/select";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import ModalBatchRename from "../../organisms/modal-batch-rename/modal-batch-rename";

interface IMenuOptionBatchRenameProps {
  readOnly: boolean;
  state: IArchiveProps;
  select: string[];
  dispatch: React.Dispatch<ArchiveAction>;
  setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>;

}

export const MenuOptionBatchRename: React.FunctionComponent<IMenuOptionBatchRenameProps> = ({
  readOnly,
  state,
  select,
  dispatch,
  setSelect
}) => {
  const [isModalBatchRename, setIsModalBatchRename] = useState(false);
  const history = useLocation();
  const undoSelection = () => new Select(select, setSelect, state, history).undoSelection();

  // Only show if files are selected
  const isVisible = select && select.length > 0;

  return (
    <>
      {isModalBatchRename && !readOnly && isVisible ? (
        <ModalBatchRename
          select={select}
          handleExit={() => {
            setIsModalBatchRename(false);
          }}
          state={state}
          isOpen={isModalBatchRename}
          dispatch={dispatch}
          undoSelection={undoSelection}
          historyLocationSearch={history.location.search}
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
