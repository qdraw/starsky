import React from 'react';
import Preloader from '../components/atoms/preloader/preloader';
import HealthStatusError from '../components/molecules/health-status-error/health-status-error';
import MenuDefault from '../components/organisms/menu-default/menu-default';
import ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import DetailViewContextWrapper from '../contexts-wrappers/detailview-wrapper';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { IArchive } from '../interfaces/IArchive';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import NotFoundPage from '../pages/not-found-page';
import Login from './login';

const MediaContent: React.FC = () => {

  var history = useLocation();
  var usesFileList = useFileList(history.location.search, false);

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;

  console.log(`-----------------MediaContent ${pageType} (rendered again)-------------------`);

  if (!usesFileList) {
    return (<><br />The application failed</>)
  }

  return (
    <div>
      <HealthStatusError />
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false} /> : null}
      {pageType === PageType.NotFound ? <NotFoundPage>not found</NotFoundPage> : null}
      {pageType === PageType.Unauthorized ? <Login /> : null}
      {pageType === PageType.ApplicationException ? <><MenuDefault isEnabled={false} />
        <div className="content--header">We hebben op dit moment een verstoring op de applicatie</div>
        <div className="content--subheader">Probeer de pagina te herladen</div></> : null}
      {pageType === PageType.Archive && archive && archive.fileIndexItems !== undefined ?
        <ArchiveContextWrapper {...archive} /> : null}
      {pageType === PageType.DetailView && detailView ? <DetailViewContextWrapper {...detailView} /> : null}
    </div>
  );
};

export default MediaContent;
