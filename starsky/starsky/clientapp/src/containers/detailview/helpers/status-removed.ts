import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { moveFolderUp } from "./move-folder-up";
import { Next } from "./next";

export function statusRemoved(
  state: IDetailView,
  relativeObjects: IRelativeObjects,
  isSearchQuery: boolean,
  history: IUseLocation,
  setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>
) {
  if (
    state.fileIndexItem?.status === IExifStatus.NotFoundSourceMissing &&
    relativeObjects.nextFilePath
  ) {
    new Next(
      relativeObjects,
      state,
      isSearchQuery,
      history,
      setRelativeObjects,
      setIsLoading
    ).next();
  } else if (
    state.fileIndexItem?.status === IExifStatus.NotFoundSourceMissing
  ) {
    moveFolderUp(new KeyboardEvent("delete"), history, isSearchQuery, state);
  }
}
