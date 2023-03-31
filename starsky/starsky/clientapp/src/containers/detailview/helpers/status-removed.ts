import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { URLPath } from "../../../shared/url-path";
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
    history.location.search.includes("%21delete%21") /* trash */ ||
    // if the update is just the wrong timeing and the file is still there
    new URLPath().StringToIUrl(history?.location?.search).f !==
      state?.fileIndexItem?.filePath
  ) {
    console.log(
      `statusRemoved: skip ${
        new URLPath().StringToIUrl(history?.location?.search).f !==
        state?.fileIndexItem?.filePath
      }`
    );
    return;
  }

  console.log("-- statusRemoved --");
  console.log(new URLPath().StringToIUrl(history?.location?.search).f);

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
