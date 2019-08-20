// import { IAppContainerState } from '../interfaces/IAppContainerState';
import { newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IMedia } from '../interfaces/IMedia';


export class CastToInterface {

  getPageType = (data: any): PageType => {
    let output: IDetailView = data;
    let pageType = PageType[output.pageType as keyof typeof PageType];
    return pageType;
  }

  MediaArchive = (data: any): IMedia<'Archive'> => {
    const media = <IMedia<'Archive'>>{
      type: 'Archive',
    }
    if (this.getPageType(data) === PageType.Archive) {
      media.data = data;
      return media;
    }
    media.data = newIArchive();
    return media;
  }

  MediaDetailView = (data: any): IMedia<'DetailView'> => {
    const media = <IMedia<'DetailView'>>{
      type: 'DetailView',
    }
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


  // AppContainerState = (data: any): IAppContainerState => {
  //   let appContainerState: IAppContainerState = data;
  //   if (!appContainerState) {
  //     return {
  //       statusCode: 999,
  //       status: ExifStatus.ServerFail,
  //       FileIndexItems: newIFileIndexItemArray(),
  //       path: '.',
  //     };
  //   }
  //   return appContainerState;
  // }

  // fileIndexItemsDetailView = (data: any): Array<IFileIndexItem> => {
  //   if (this.getPageType(data) !== PageType.DetailView) return new Array<IFileIndexItem>();
  //   let DetailView: IDetailView = data;
  //   return new Array<IFileIndexItem>(DetailView.fileIndexItem);
  // }

  // fileIndexItemsArchive = (data: any): Array<IFileIndexItem> => {
  //   if (this.getPageType(data) !== PageType.Archive) return new Array<IFileIndexItem>();
  //   let archive: IArchive = data;
  //   // console.log(archive.fileIndexItems);
  //   return archive.fileIndexItems;
  // }

  // AppContainerStateWithDefaults = (responseObject: Object | null): IAppContainerState => {
  //   var fallBack = {
  //     statusCode: 999,
  //     status: ExifStatus.ServerFail,
  //     FileIndexItems: newIFileIndexItemArray(),
  //     path: '.',
  //   };

  //   if (!responseObject) {
  //     return fallBack;
  //   }

  //   var response = new CastToInterface().AppContainerState(responseObject);
  //   switch (response.status) {
  //     case 404: {
  //       return {
  //         statusCode: 404,
  //         FileIndexItems: newIFileIndexItemArray(),
  //         status: ExifStatus.NotFoundNotInIndex
  //       };
  //     }
  //     case 401: {
  //       return {
  //         statusCode: 401,
  //         FileIndexItems: newIFileIndexItemArray(),
  //         status: ExifStatus.Unauthorized
  //       };
  //     }
  //     case 504: {
  //       return {
  //         statusCode: 504,
  //         FileIndexItems: newIFileIndexItemArray(),
  //         status: ExifStatus.ServerFail
  //       };
  //     }
  //   }

  //   let pageType = new CastToInterface().getPageType(response);

  //   if (pageType === PageType.DetailView) {
  //     console.log("those.setState >= DetailView");
  //     return {
  //       statusCode: 200,
  //       PageType: PageType.DetailView,
  //       FileIndexItems: new CastToInterface().fileIndexItemsDetailView(response),
  //       detailView: response.detailView,
  //       path: 'subPathdetailView',
  //       status: ExifStatus.Ok
  //     };
  //   }
  //   else if (pageType === PageType.Archive) {
  //     console.log('response', response);

  //     return {
  //       statusCode: 200,
  //       PageType: PageType.Archive,
  //       FileIndexItems: new CastToInterface().fileIndexItemsArchive(response),
  //       path: 'subPathArchive',
  //       archive: response.archive,
  //       status: ExifStatus.Ok
  //     };
  //   }
  //   return fallBack;
  // }

}
