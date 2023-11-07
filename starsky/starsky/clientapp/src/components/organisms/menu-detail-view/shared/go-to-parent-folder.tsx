import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { INavigateState } from "../../../../interfaces/INavigateState";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { UrlQuery } from "../../../../shared/url-query";

export interface IGoToParentFolderProps {
  isSearchQuery: boolean;
  history: IUseLocation;
  state: IDetailView;
}

export const GoToParentFolder: React.FunctionComponent<
  IGoToParentFolderProps
> = ({ isSearchQuery, history, state }) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageGoToParentFolder = language.key(
    localization.MessageGoToParentFolder
  );

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
        <li
          className="menu-option"
          data-test="go-to-parent-folder"
          onClick={() => navigateToParentFolder()}
          onKeyDown={(event) => {
            event.key === "Enter" && navigateToParentFolder();
          }}
        >
          {MessageGoToParentFolder}
        </li>
      ) : null}
    </>
  );
};
