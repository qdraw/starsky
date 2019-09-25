// import { IAppContainerState } from '../interfaces/IAppContainerState';
import { newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IMedia } from '../interfaces/IMedia';


export class CastToInterface {

  /**
   * Get the type of the page by the object
   */
  getPageType = (data: any): PageType => {
    if (!data) return PageType.ApplicationException;

    let output: IDetailView = data;
    let pageType = PageType[output.pageType as keyof typeof PageType];
    return pageType;
  }

  /**
   * Plain JS-object to casted object for Archive, Search and Trash pages
   */
  MediaArchive = (data: any): IMedia<'Archive'> => {
    const media = {
      type: 'Archive',
    } as IMedia<'Archive'>

    if (this.getPageType(data) === PageType.Archive || this.getPageType(data) === PageType.Search || this.getPageType(data) === PageType.Trash) {
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

    if (this.getPageType(data) === PageType.DetailView) {
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

}
