import React, { useContext } from 'react';
import Breadcrumb from '../components/breadcrumbs';
import ColorClassFilter from '../components/color-class-filter';
import ItemListSelect from '../components/item-list-select';
import ItemListView from '../components/item-list-view';
import RelativeLink from '../components/relative-link';
import HistoryContext from '../contexts/history-contexts';
import { IRelativeObjects } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';

interface IArchiveProps {
  fileIndexItems: Array<IFileIndexItem>;
  relativeObjects: IRelativeObjects;
  subPath: string;
  breadcrumb: Array<string>;
  colorClassFilterList: Array<number>;
  colorClassUsage: Array<number>;
  collectionsCount: number;
}

function Archive(archive: IArchiveProps) {

  const history = useContext(HistoryContext);
  const sidebar = new URLPath().StringToIUrl(history.location.hash).sidebar;

  if (!archive.colorClassUsage) return (<>no colorClassUsage</>)

  return (
    <div className="archive collapsed">

      <div className="content">

        <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath}></Breadcrumb>
        <RelativeLink relativeObjects={archive.relativeObjects}></RelativeLink>

        <ColorClassFilter itemsCount={archive.collectionsCount} subPath={archive.subPath}
          colorClassFilterList={archive.colorClassFilterList} colorClassUsage={archive.colorClassUsage}></ColorClassFilter>

        {!sidebar ? <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView> : null}
        {sidebar ? <ItemListSelect {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListSelect> : null}

      </div>

    </div>
  )
}
export default Archive;
