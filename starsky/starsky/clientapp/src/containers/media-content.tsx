import Notification, { NotificationType } from "../components/atoms/notification/notification";
import Preloader from "../components/atoms/preloader/preloader";
import HealthStatusError from "../components/molecules/health-status-error/health-status-error";
import ApplicationException from "../components/organisms/application-exception/application-exception";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import DetailViewContextWrapper from "../contexts-wrappers/detailview-wrapper";
import useSockets from "../hooks/realtime/use-sockets";
import useFileList from "../hooks/use-filelist";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location/use-location";
import { IArchive } from "../interfaces/IArchive";
import { IDetailView, PageType } from "../interfaces/IDetailView";
import localization from "../localization/localization.json";
import { NotFoundPage } from "../pages/not-found-page";
import { Language } from "../shared/language";
import { Login } from "./login";
import HealthCheckForUpdates from "../components/molecules/health-check-for-updates/health-check-for-updates.tsx";

const MediaContent: React.FC = () => {
  const history = useLocation();
  const usesFileList = useFileList(history.location.search, false);

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;

  const { showSocketError, setShowSocketError } = useSockets();

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageConnectionRealtimeError = language.key(localization.MessageConnectionRealtimeError);
  const MessageApplicationFailed = language.key(localization.MessageApplicationFailed);

  console.log(`-----------------MediaContent ${pageType} (rendered again)-------------------`);

  if (!usesFileList) {
    return (
      <>
        <br />
        {MessageApplicationFailed}
      </>
    );
  }

  return (
    <div>
      {showSocketError ? (
        <Notification type={NotificationType.default} callback={() => setShowSocketError(null)}>
          {MessageConnectionRealtimeError}
        </Notification>
      ) : null}
      <HealthStatusError />
      <HealthCheckForUpdates />
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isWhite={false} /> : null}
      {pageType === PageType.NotFound ? <NotFoundPage /> : null}
      {pageType === PageType.Unauthorized ? <Login /> : null}
      {pageType === PageType.ApplicationException ? (
        <ApplicationException></ApplicationException>
      ) : null}
      {pageType === PageType.Archive && archive?.fileIndexItems ? (
        <ArchiveContextWrapper {...archive} />
      ) : null}
      {pageType === PageType.DetailView && detailView ? (
        <DetailViewContextWrapper {...detailView} />
      ) : null}
    </div>
  );
};

export default MediaContent;
