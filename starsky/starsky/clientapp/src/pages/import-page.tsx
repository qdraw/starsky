
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import DropArea from '../components/drop-area';
import MenuSearch from '../components/menu-search';
import ModalDropAreaFilesAdded from '../components/modal-drop-area-files-added';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import DocumentTitle from '../shared/document-title';
import { UrlQuery } from '../shared/url-query';

const ImportPage: FunctionComponent<RouteComponentProps> = (props) => {

  document.title = new DocumentTitle().GetDocumentTitle("Import");

  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(newIFileIndexItemArray());

  return (<div>
    {/* DropArea output modal */}
    {dropAreaUploadFilesList.length !== 0 ? <ModalDropAreaFilesAdded
      handleExit={() => setDropAreaUploadFilesList(newIFileIndexItemArray())}
      uploadFilesList={dropAreaUploadFilesList}
      isOpen={dropAreaUploadFilesList.length !== 0} /> : null}

    <MenuSearch></MenuSearch>
    <div className="content">
      <div className="content--header">Importeren</div>
      <div className="content--subheader">
        <DropArea callback={(add) => {
          setDropAreaUploadFilesList(add);
        }}
          endpoint={new UrlQuery().UrlImportApi()}
          enableInputButton={true}
          className="btn btn--default"
          enableDragAndDrop={true}></DropArea>
      </div>
    </div>
  </div>
  )
}

export default ImportPage;
