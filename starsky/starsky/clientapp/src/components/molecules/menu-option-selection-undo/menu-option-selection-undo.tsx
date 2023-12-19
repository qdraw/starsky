import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOption from "../../atoms/menu-option/menu-option";

export interface IMenuOptionSelectionUndoProps {
  select: string[];
  state?: IArchiveProps;
  undoSelection: () => void;
}

export const MenuOptionSelectionUndo: React.FunctionComponent<
  IMenuOptionSelectionUndoProps
> = ({ select, state, undoSelection }) => {
  return (
    <>
      {select.length === state?.fileIndexItems?.length ? (
        <MenuOption
          isReadOnly={false}
          localization={localization.MessageUndoSelection}
          onClickKeydown={() => undoSelection()}
          testName="undo-selection"
        />
      ) : null}
    </>
  );
};
