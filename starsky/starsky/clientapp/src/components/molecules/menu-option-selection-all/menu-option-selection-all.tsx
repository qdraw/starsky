import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import MenuOption from "../../atoms/menu-option/menu-option";

interface IMenuOptionUndoSelectionProps {
  select: string[];
  state?: IArchiveProps;
  allSelection: () => void;
}

export const MenuOptionSelectionAll: React.FunctionComponent<IMenuOptionUndoSelectionProps> = ({
  select,
  state,
  allSelection
}) => {
  return (
    <>
      {select.length !== state?.fileIndexItems?.length ? (
        <MenuOption
          isReadOnly={false}
          testName="select-all"
          localization={localization.MessageSelectAll}
          onClickKeydown={() => allSelection()}
        />
      ) : null}
    </>
  );
};
