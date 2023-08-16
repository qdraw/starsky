import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { moveFolderUp } from "./move-folder-up";
import { PrevNext } from "./prev-next";

export function statusRemoved(
  state: IDetailView,
  relativeObjects: IRelativeObjects,
  isSearchQuery: boolean,
  history: IUseLocation,
  setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>
) {
  if (
    !state.fileIndexItem?.status ||
    history.location.search.includes("!delete!") ||
    history.location.search.includes("%21delete%21") /* trash */
  ) {
    return;
  }

  if (
    (state.fileIndexItem?.status === IExifStatus.NotFoundSourceMissing ||
      state.fileIndexItem?.status === IExifStatus.Deleted) &&
    relativeObjects.nextFilePath
  ) {
    new PrevNext(
      relativeObjects,
      state,
      isSearchQuery,
      history,
      setRelativeObjects,
      setIsLoading
    ).next();
  } else if (
    state.fileIndexItem?.status === IExifStatus.NotFoundSourceMissing ||
    state.fileIndexItem?.status === IExifStatus.Deleted
  ) {
    moveFolderUp(new KeyboardEvent("delete"), history, isSearchQuery, state);
  }
}
