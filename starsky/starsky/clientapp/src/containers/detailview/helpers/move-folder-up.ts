import { ModalOpenClassName } from "../../../components/atoms/modal/modal";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../interfaces/IDetailView";
import { INavigateState } from "../../../interfaces/INavigateState";
import { Keyboard } from "../../../shared/keyboard";
import { UrlQuery } from "../../../shared/url/url-query";

export function moveFolderUp(
  event: KeyboardEvent,
  history: IUseLocation,
  isSearchQuery: boolean,
  state: IDetailView
) {
  if (!history.location) return;
  if (new Keyboard().isInForm(event)) return;

  const isPortalActive = !!(document.querySelector(`.${ModalOpenClassName}`) as HTMLElement);
  if (isPortalActive) {
    return;
  }
  const url = isSearchQuery
    ? new UrlQuery().HashSearchPage(history.location.search)
    : new UrlQuery().updateFilePathHash(
        history.location.search,
        state.fileIndexItem.parentDirectory
      );

  history.navigate(url, {
    state: {
      filePath: state.fileIndexItem.filePath
    } as INavigateState
  });
}
