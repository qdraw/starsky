
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import DropArea from '../components/atoms/drop-area/drop-area';
import MenuDefault from '../components/organisms/menu-default/menu-default';
import ModalDropAreaFilesAdded from '../components/organisms/modal-drop-area-files-added/modal-drop-area-files-added';
import useGlobalSettings from '../hooks/use-global-settings';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import DocumentTitle from '../shared/document-title';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';

const ImportPage: FunctionComponent<RouteComponentProps> = (props) => {

  document.title = new DocumentTitle().GetDocumentTitle("Import");

  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(newIFileIndexItemArray());

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageImportName = language.text("Importeren", "Import");

  return (<div>
    {/* DropArea output modal */}
    {dropAreaUploadFilesList.length !== 0 ? <ModalDropAreaFilesAdded
      handleExit={() => setDropAreaUploadFilesList(newIFileIndexItemArray())}
      uploadFilesList={dropAreaUploadFilesList}
      isOpen={dropAreaUploadFilesList.length !== 0} /> : null}

    <MenuDefault isEnabled={true}></MenuDefault>
    <div className="content">
      <div className="content--header">{MessageImportName}</div>
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
