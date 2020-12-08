import React, { useEffect, useState } from "react";
import Notification, {
  NotificationType
} from "../components/atoms/notification/notification";
import Preloader from "../components/atoms/preloader/preloader";
import HealthStatusError from "../components/molecules/health-status-error/health-status-error";
import ApplicationException from "../components/organisms/application-exception/application-exception";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import DetailViewContextWrapper from "../contexts-wrappers/detailview-wrapper";
import useSockets from "../hooks/realtime/use-sockets";
import useFileList from "../hooks/use-filelist";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location";
import { IArchive } from "../interfaces/IArchive";
import { IDetailView, PageType } from "../interfaces/IDetailView";
import NotFoundPage from "../pages/not-found-page";
import { Language } from "../shared/language";
import Login from "./login";

const MediaContent: React.FC = () => {
  var history = useLocation();
  var usesFileList = useFileList(history.location.search, false);

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList
    ? usesFileList.archive
    : undefined;
  const detailView: IDetailView | undefined = usesFileList
    ? usesFileList.detailView
    : undefined;

  const { showSocketError } = useSockets();
  const [connectionNotification, setConnectNotification] = useState(false);

  useEffect(() => {
    if (showSocketError == null) return;
    setConnectNotification(showSocketError);
  }, [showSocketError]);

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageConnectionRealtimeError = language.text(
    "De verbinding is niet helemaal oké. We proberen het te herstellen",
    "The connection is not quite right. We are trying to fix it"
  );

  console.log(
    `-----------------MediaContent ${pageType} (rendered again)-------------------`
  );

  if (!usesFileList) {
    return (
      <>
        <br />
        The application has failed. Please reload it to try it again
      </>
    );
  }

  return (
    <div>
      {connectionNotification ? (
        <Notification
          type={NotificationType.default}
          callback={() => setConnectNotification(false)}
        >
          {MessageConnectionRealtimeError}
        </Notification>
      ) : null}
      <HealthStatusError />
      {pageType === PageType.Loading ? (
        <Preloader isOverlay={true} isDetailMenu={false} />
      ) : null}
      {pageType === PageType.NotFound ? (
        <NotFoundPage>not found</NotFoundPage>
      ) : null}
      {pageType === PageType.Unauthorized ? <Login /> : null}
      {pageType === PageType.ApplicationException ? (
        <ApplicationException></ApplicationException>
      ) : null}
      {pageType === PageType.Archive &&
      archive &&
      archive.fileIndexItems !== undefined ? (
        <ArchiveContextWrapper {...archive} />
      ) : null}
      {pageType === PageType.DetailView && detailView ? (
        <DetailViewContextWrapper {...detailView} />
      ) : null}
    </div>
  );
};

export default MediaContent;
