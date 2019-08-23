import React from 'react';
import Breadcrumb from '../components/breadcrumbs';
import ColorClassFilter from '../components/color-class-filter';
import ItemListView from '../components/item-list-view';
import RelativeLink from '../components/relative-link';
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
  searchQuery: string;
}

function Archive(archive: IArchiveProps) {

  const sidebar = new URLPath().StringToIUrl(archive.subPath).sidebar;

  if (!archive.colorClassUsage) return (<>no colorClassUsage</>)

  return (
    <div className="archive collapsed">

      <div className="content">

        <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath}></Breadcrumb>
        <RelativeLink relativeObjects={archive.relativeObjects}></RelativeLink>

        <ColorClassFilter itemsCount={archive.collectionsCount} subPath={archive.subPath}
          colorClassFilterList={archive.colorClassFilterList} colorClassUsage={archive.colorClassUsage}></ColorClassFilter>

        <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView>
      </div>

    </div>
  )
}
export default Archive;
