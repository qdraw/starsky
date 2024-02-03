import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { INavigateState } from "../../../../interfaces/INavigateState";
import localization from "../../../../localization/localization.json";
import { UrlQuery } from "../../../../shared/url-query";
import MenuOption from "../../../atoms/menu-option/menu-option";

export interface IGoToParentFolderProps {
  isSearchQuery: boolean;
  history: IUseLocation;
  state: IDetailView;
}

export const GoToParentFolder: React.FunctionComponent<IGoToParentFolderProps> = ({
  isSearchQuery,
  history,
  state
}) => {
  function navigateToParentFolder() {
    history.navigate(
      new UrlQuery().updateFilePathHash(
        history.location.search,
        state.fileIndexItem.parentDirectory,
        true
      ),
      {
        state: {
          filePath: state.fileIndexItem.filePath
        } as INavigateState
      }
    );
  }

  return (
    <>
      {isSearchQuery ? (
        <MenuOption
          isReadOnly={false}
          onClickKeydown={() => navigateToParentFolder()}
          testName="go-to-parent-folder"
          localization={localization.MessageGoToParentFolder}
        />
      ) : null}
    </>
  );
};
