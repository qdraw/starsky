import React from 'react';
import Preloader from '../components/preloader';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { PageType } from '../interfaces/IDetailView';
import Archive from './archive';
import DetailView from './detailview';
import Menu from './menu';

interface IMediaContentProps {
}

const MediaContent: React.FC<IMediaContentProps> = (props) => {
  console.log('-----------------MediaContent (rendered again)-------------------');

  var location = useLocation();
  var uses = useFileList(location.location.search);

  const { parent, pageType, archive, detailView } = uses;

  return (
    <div>
      <Menu parent={parent} isDetailMenu={pageType === PageType.DetailView}></Menu>
      {pageType === PageType.Loading ? <Preloader isOverlay={true} isDetailMenu={false} ></Preloader> : null}
      {pageType === PageType.Unknown ? <>not found</> : null}
      {pageType === PageType.Archive ? <Archive {...archive} /> : null}
      {pageType === PageType.DetailView ? <DetailView {...detailView} /> : null}
    </div>
  );
}

export default MediaContent;
