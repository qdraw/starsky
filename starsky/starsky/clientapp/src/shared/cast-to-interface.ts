import { IArchive, newIArchive } from "../interfaces/IArchive";
import { IDetailView, newDetailView, PageType } from "../interfaces/IDetailView";
import { IFileIndexItem } from "../interfaces/IFileIndexItem";
import { IMedia } from "../interfaces/IMedia";

export class CastToInterface {
  /**
   * Plain JS-object to casted object for Archive, Search and Trash pages
   */
  MediaArchive = (data: unknown): IMedia<"Archive"> => {
    const castedData = data as IArchive;
    const media = {
      type: "Archive"
    } as IMedia<"Archive">;

    if (
      castedData.pageType === PageType.Archive ||
      castedData.pageType === PageType.Search ||
      castedData.pageType === PageType.Trash
    ) {
      media.data = castedData;
      return media;
    }
    // default situation
    media.data = newIArchive();
    return media;
  };

  /**
   * Plain JS-object to casted object for DetailView pages
   */
  MediaDetailView = (data: unknown): IMedia<"DetailView"> => {
    const media = {
      type: "DetailView"
    } as IMedia<"DetailView">;

    const castedData = data as IDetailView;

    if (castedData.pageType === PageType.DetailView) {
      media.data = castedData;
      return media;
    }
    // default situation
    media.data = newDetailView();
    return media;
  };

  /**
   * Return casted list
   */
  InfoFileIndexArray = (data: unknown): Array<IFileIndexItem> => {
    if (typeof data === "string") return [];
    return data as Array<IFileIndexItem>;
  };

  /**
   * state without any context
   */
  UndefinedIArchiveReadonly = (state: IArchive | undefined): IArchive => {
    if (state === undefined) {
      state = newIArchive();
      state.isReadOnly = true;
    }
    return state;
  };
}
