import React from 'react';
import Preloader from '../components/preloader';
import ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import DetailViewContextWrapper from '../contexts-wrappers/detailview-wrapper';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { IArchive } from '../interfaces/IArchive';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import Login from './login';


const MediaContent: React.FC = () => {
  console.log('-----------------MediaContent (rendered again)-------------------');

  var history = useLocation();
  var usesFileList = useFileList(history.location.search);

  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;
  if (!usesFileList) {
    return (<><br />The application failed</>)
  }

  return (
    <div>
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false}></Preloader> : null}
      {pageType === PageType.NotFound ? <>not found</> : null}
      {pageType === PageType.Unauthorized ? <Login></Login> : null}
      {pageType === PageType.ApplicationException ? <>ApplicationException</> : null}
      {pageType === PageType.Archive && archive ? <ArchiveContextWrapper {...archive} /> : null}
      {pageType === PageType.DetailView && detailView ? <DetailViewContextWrapper {...detailView} /> : null}
    </div>
  );
}

export default MediaContent;
