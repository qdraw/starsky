import React from 'react';
import Preloader from '../components/preloader';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { IArchive } from '../interfaces/IArchive';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import Archive from './archive';
import DetailView from './detailview';
import Menu from './menu';


const MediaContent: React.FC = () => {
  console.log('-----------------MediaContent (rendered again)-------------------');

  var history = useLocation();
  var usesFileList = useFileList(history.location.search);

  const parent = usesFileList ? usesFileList.parent : "/?";
  const pageType = usesFileList ? usesFileList.pageType : PageType.Loading;
  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;

  if (!usesFileList) {
    return (<><br />The application failed</>)
  }

  return (
    <div>
      <Menu parent={parent} isDetailMenu={pageType === PageType.DetailView}></Menu>
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false} >tttt</Preloader> : null}
      {pageType === PageType.NotFound ? <>not found</> : null}
      {pageType === PageType.ApplicationException ? <>ApplicationException</> : null}
      {pageType === PageType.Archive && archive ? <Archive {...archive} /> : null}
      {pageType === PageType.DetailView && detailView ? <DetailView {...detailView} /> : null}
    </div>
  );
}

export default MediaContent;
