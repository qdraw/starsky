import React from 'react';
import Notification, { NotificationType } from '../components/atoms/notification/notification';
import Preloader from '../components/atoms/preloader/preloader';
import HealthStatusError from '../components/molecules/health-status-error/health-status-error';
import MenuDefault from '../components/organisms/menu-default/menu-default';
import ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import DetailViewContextWrapper from '../contexts-wrappers/detailview-wrapper';
import useFileList from '../hooks/use-filelist';
import useFileListCacheWatcher from '../hooks/use-filelist-cache-watcher';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import useSockets from '../hooks/use-sockets';
import { IArchive } from '../interfaces/IArchive';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import NotFoundPage from '../pages/not-found-page';
import { Language } from '../shared/language';
import Login from './login';

const MediaContent: React.FC = () => {

  var history = useLocation();
  var usesFileList = useFileList(history.location.search, false);
  const { socketsFailed } = useSockets();
  useFileListCacheWatcher();

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageConnectionRealtimeError = language.text("Er zijn verbindingsproblemen, " +
    "realtime verversen staat hierdoor uit.",
    "There are connection problems, real-time refresh is disabled because of this. ");
  const MessageRefreshPageTryAgain = language.text("Herlaad de pagina om het opnieuw te proberen",
    "Please reload the page to try again");

  console.log(`-----------------MediaContent ${pageType} (rendered again)-------------------`);

  if (!usesFileList) {
    return (<><br />The application failed</>)
  }

  return (
    <div>
      {socketsFailed ? <Notification type={NotificationType.default}>{MessageConnectionRealtimeError}&nbsp;
        {MessageRefreshPageTryAgain}</Notification> : null}
      <HealthStatusError />
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false} /> : null}
      {pageType === PageType.NotFound ? <NotFoundPage>Page Not found</NotFoundPage> : null}
      {pageType === PageType.Unauthorized ? <Login /> : null}
      {pageType === PageType.ApplicationException ? <><MenuDefault isEnabled={false} />
        <div className="content--header">We hebben op dit moment een verstoring op de applicatie</div>
        <div className="content--subheader">{MessageRefreshPageTryAgain}</div></> : null}
      {pageType === PageType.Archive && archive && archive.fileIndexItems !== undefined ?
        <ArchiveContextWrapper {...archive} /> : null}
      {pageType === PageType.DetailView && detailView ? <DetailViewContextWrapper {...detailView} /> : null}
    </div>
  );
};

export default MediaContent;
