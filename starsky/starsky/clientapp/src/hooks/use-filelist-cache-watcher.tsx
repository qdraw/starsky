import { useEffect } from 'react';
import { IArchive } from '../interfaces/IArchive';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { FileListCache } from '../shared/filelist-cache';
import { useSocketsEventName } from './use-sockets';

export interface IUseFileListCacheWatcher {
}

const useFileListCacheWatcher = (): IUseFileListCacheWatcher => {

  useEffect(() => {
    function updateDetailView(event: Event) {
      const pushMessage = event as CustomEvent<IFileIndexItem>;

      var cache = new FileListCache().CacheGet(`?/f${pushMessage.detail.parentDirectory}`);
      if (!cache) return;

      switch (cache.pageType) {
        case PageType.Archive:
          var archive = cache as IArchive;
          archive.fileIndexItems.forEach(item => {
            if (item.filePath === pushMessage.detail.filePath) {
              item = pushMessage.detail;
              // todo finish
              return;
            }
          });
          break;
        case PageType.DetailView:
          var detailView = cache as IDetailView;
          detailView.fileIndexItem = pushMessage.detail;
          break;
        default:
          break;
      }

    }

    document.body.addEventListener(useSocketsEventName, updateDetailView);
    return () => {
      document.body.removeEventListener(useSocketsEventName, updateDetailView);
    };
    // only when start of view
    // eslint-disable-next-line
  }, []);

  return {};
};

export default useFileListCacheWatcher;