import React, { FunctionComponent } from "react";
import DropArea from "../../components/atoms/drop-area/drop-area";
import ModalDropAreaFilesAdded from "../../components/molecules/modal-drop-area-files-added/modal-drop-area-files-added";
import MenuDefault from "../../components/organisms/menu-default/menu-default";
import useGlobalSettings from "../../hooks/use-global-settings";
import { newIFileIndexItemArray } from "../../interfaces/IFileIndexItem";
import localization from "../../localization/localization.json";
import { DocumentTitle } from "../../shared/document-title";
import { Language } from "../../shared/language";
import { UrlQuery } from "../../shared/url-query";

export const Import: FunctionComponent = () => {
  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] =
    React.useState(newIFileIndexItemArray());

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageImportHeader = language.key(localization.MessageImportHeader);
  document.title = new DocumentTitle().GetDocumentTitle(MessageImportHeader);

  return (
    <div>
      {/* DropArea output modal */}
      {dropAreaUploadFilesList.length !== 0 ? (
        <ModalDropAreaFilesAdded
          handleExit={() => setDropAreaUploadFilesList(newIFileIndexItemArray())}
          uploadFilesList={dropAreaUploadFilesList}
          isOpen={dropAreaUploadFilesList.length !== 0}
        />
      ) : null}

      <MenuDefault isEnabled={true}></MenuDefault>
      <div className="content">
        <div className="content--header">{MessageImportHeader}</div>
        <div className="content--subheader">
          <DropArea
            callback={(add) => {
              setDropAreaUploadFilesList(add);
            }}
            endpoint={new UrlQuery().UrlImportApi()}
            enableInputButton={true}
            className="btn btn--default"
            enableDragAndDrop={true}
          ></DropArea>
        </div>
      </div>
    </div>
  );
};
