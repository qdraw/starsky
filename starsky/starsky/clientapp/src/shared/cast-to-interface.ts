// import { IAppContainerState } from '../interfaces/IAppContainerState';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { newDetailView, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IMedia } from '../interfaces/IMedia';


export class CastToInterface {

  /**
   * Plain JS-object to casted object for Archive, Search and Trash pages
   */
  MediaArchive = (data: any): IMedia<'Archive'> => {
    const media = {
      type: 'Archive',
    } as IMedia<'Archive'>

    if (data.pageType === PageType.Archive || data.pageType === PageType.Search || data.pageType === PageType.Trash) {
      media.data = data;
      return media;
    }
    // default situation
    media.data = newIArchive();
    return media;
  }

  /**
  * Plain JS-object to casted object for DetailView pages
  */
  MediaDetailView = (data: any): IMedia<'DetailView'> => {
    const media = {
      type: 'DetailView',
    } as IMedia<'DetailView'>

    if (data.pageType === PageType.DetailView) {
      media.data = data;
      return media;
    }
    // default situation
    media.data = newDetailView();
    return media;
  }

  /**
   * Return casted list
   */
  InfoFileIndexArray = (data: any): Array<IFileIndexItem> => {
    return data as Array<IFileIndexItem>;
  }

  /**
   * state without any context
   */
  UndefinedIArchiveReadonly = (state: IArchive | undefined): IArchive => {
    if (state === undefined) {
      state = newIArchive();
      state.isReadOnly = true;
    }
    return state;
  }

}
