import React, { useEffect } from 'react';
import useGlobalSettings from '../hooks/use-globalSettings';
import useInterval from '../hooks/use-interval';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import Modal from './modal';

interface IModalTrashProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: Function;
}
// { isModalExportOpen ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[detailView.subPath]} isOpen={isModalExportOpen} /> : null }

enum ProcessingState {
  default,
  server,
  ready,
  fail
}

const ModalExport: React.FunctionComponent<IModalTrashProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDownloadSelection = language.text("Download selectie", "Download selection");
  const MessageOrginalFile = language.text("Origineel bestand", "Original file");
  const MessageThumbnailFile = "Thumbnail";
  const MessageGenericExportFail = language.text("Er is iets misgegaan met exporteren", "Something went wrong with exporting");
  const MessageExportReady = language.text("Het bestand {createZipKey} is klaar met exporteren.", "The file {createZipKey} has finished exporting.");
  const MessageDownloadAsZipArchive = language.text("Download als zip-archief", "Download as a zip archive");

  const [isProcessing, setProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");

  async function postZip(isThumbnail: boolean) {
    if (!props.select) {
      setProcessing(ProcessingState.fail);
      return;
    }
    /*
    f	/__starsky/0001-readonly/4.jpg;/__starsky/0001-readonly/3.jpg
    json	true
    thumbnail	false
    collections	true
    */
    var bodyParams = new URLSearchParams();

    var selectString = "";
    for (let index = 0; index < props.select.length; index++) {
      const element = props.select[index];
      if (index === 0) {
        selectString = element;
        continue;
      }
      selectString += ";" + element;
    }

    bodyParams.set("f", selectString);
    bodyParams.set("json", "true");
    bodyParams.set("thumbnail", isThumbnail.toString());
    bodyParams.set("collections", "true");
    setProcessing(ProcessingState.server);

    var zipKeyResult = await FetchPost(new UrlQuery().UrlExportPostZipApi(), bodyParams.toString());

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
  }

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    if (!createZipKey) return;
    var result = await FetchGet(new UrlQuery().UrlExportZipApi(createZipKey, true));
    if (result.statusCode === 200) {
      setProcessing(ProcessingState.ready);
      return;
    }
    setProcessing(ProcessingState.fail);
  }, 1500);


  const [singleFileThumbnailStatus, setSingleFileThumbnailStatus] = React.useState(true);

  useEffect(() => {
    /* some filetypes don't allow to be thumnailed by the backend server */
    async function getThumbnailSingleFileStatus() {
      if (!props.select || props.select.length !== 1) return;

      var result = await FetchGet(new UrlQuery().UrlDownloadPhotoApi(props.select[0], true));

      if (result && result.statusCode && result.statusCode === 500) {
        setSingleFileThumbnailStatus(false);
        return;
      }
      setSingleFileThumbnailStatus(true);
    }
    getThumbnailSingleFileStatus();
  }, [props]);

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>

    <div className="modal content--subheader">{MessageDownloadSelection}</div>
    <div className="modal content--text">
      {isProcessing === ProcessingState.default && props.select && props.select.length === 1 ? <>
        <a href={new UrlQuery().UrlDownloadPhotoApi(props.select[0], false)} download={new URLPath().FileNameBreadcrumb(props.select[0])}
          target="_blank" rel="noopener noreferrer" className="btn btn--info">Orgineel</a>
        {singleFileThumbnailStatus ? <a href={new UrlQuery().UrlDownloadPhotoApi(props.select[0], true)} download={new URLPath().FileNameBreadcrumb(props.select[0])}
          target="_blank" rel="noopener noreferrer" className={"btn btn--default"}>Thumbnail</a> : null}
      </> : null}

      {isProcessing === ProcessingState.default && props.select && props.select.length >= 2 ? <>
        <button onClick={() => {
          postZip(false)
        }} className="btn btn--info">{MessageOrginalFile}</button>

        <button onClick={() => {
          postZip(true)
        }} className="btn btn--default">{MessageThumbnailFile}</button>
      </> : null}

      {isProcessing === ProcessingState.server ? <>
        <b>Een moment geduld s.v.p.</b><br />
        Op de achtergrond worden de afbeeldingen klaar gezet
      </> : null}

      {isProcessing === ProcessingState.fail ? MessageGenericExportFail : null}

      {isProcessing === ProcessingState.ready ? <>
        {language.token(MessageExportReady, ["{createZipKey}"], [createZipKey])}
        <a className="btn btn--default" href={new UrlQuery().UrlExportZipApi(createZipKey, false)} download rel="noopener noreferrer" target="_blank">
          {MessageDownloadAsZipArchive}
        </a>
      </> : null}
    </div>
  </Modal>)
};

export default ModalExport
