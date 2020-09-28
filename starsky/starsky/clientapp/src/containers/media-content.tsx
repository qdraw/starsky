import React from 'react';
import Notification, { NotificationType } from '../components/atoms/notification/notification';
import Preloader from '../components/atoms/preloader/preloader';
import HealthStatusError from '../components/molecules/health-status-error/health-status-error';
import MenuDefault from '../components/organisms/menu-default/menu-default';
import ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import DetailViewContextWrapper from '../contexts-wrappers/detailview-wrapper';
import useFileList from '../hooks/use-filelist';
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

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;
  const { showSocketError } = useSockets();

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageApplicationException = language.text("We hebben een op dit moment een verstoring op de applicatie", "We have a disruption on the application right now")
  const MessageRefreshPageTryAgain = language.text("Herlaad de pagina om het opnieuw te proberen",
    "Please reload the page to try again");
  const MessageConnectionRealtimeError = language.text("De verbinding is niet helemaal oké. " +
    "We proberen het te herstellen",
    "The connection is not quite right. We are trying to fix it");

  console.log(`-----------------MediaContent ${pageType} (rendered again)-------------------`);

  if (!usesFileList) {
    return (<><br />The application failed</>)
  }

  return (
    <div>
      {showSocketError ? <Notification type={NotificationType.default}>{MessageConnectionRealtimeError}</Notification> : null}
      <HealthStatusError />
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false} /> : null}
      {pageType === PageType.NotFound ? <NotFoundPage>not found</NotFoundPage> : null}
      {pageType === PageType.Unauthorized ? <Login /> : null}
      {pageType === PageType.ApplicationException ? <><MenuDefault isEnabled={false} />
        <div className="content--header">{MessageApplicationException}</div>
        <div className="content--subheader">{MessageRefreshPageTryAgain}</div></> : null}
      {pageType === PageType.Archive && archive && archive.fileIndexItems !== undefined ?
        <ArchiveContextWrapper {...archive} /> : null}
      {pageType === PageType.DetailView && detailView ? <DetailViewContextWrapper {...detailView} /> : null}
    </div>
  );
};

export default MediaContent;
