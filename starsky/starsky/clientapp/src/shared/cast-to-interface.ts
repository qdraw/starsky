// import { IAppContainerState } from '../interfaces/IAppContainerState';
import { newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IMedia } from '../interfaces/IMedia';


export class CastToInterface {

  getPageType = (data: any): PageType => {
    if (!data) return PageType.ApplicationException;

    let output: IDetailView = data;
    let pageType = PageType[output.pageType as keyof typeof PageType];
    if (data.searchQuery && data.searchQuery === "!delete!") {
      pageType = PageType.Trash;
    }
    return pageType;
  }

  MediaArchive = (data: any): IMedia<'Archive'> => {
    const media = {
      type: 'Archive',
    } as IMedia<'Archive'>

    if (this.getPageType(data) === PageType.Archive || this.getPageType(data) === PageType.Search || this.getPageType(data) === PageType.Trash) {
      media.data = data;
      return media;
    }
    media.data = newIArchive();
    return media;
  }

  MediaDetailView = (data: any): IMedia<'DetailView'> => {
    const media = {
      type: 'DetailView',
    } as IMedia<'DetailView'>

    if (this.getPageType(data) === PageType.DetailView) {
      media.data = data;
      return media;
    }
    media.data = newDetailView();
    return media;
  }

  InfoFileIndexArray = (data: any): Array<IFileIndexItem> => {
    return data as Array<IFileIndexItem>;
  }

}
