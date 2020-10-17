import React, { useEffect } from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useInterval from '../../../hooks/use-interval';
import { PageType } from '../../../interfaces/IDetailView';
import { ExportIntervalUpdate } from '../../../shared/export/export-interval-update';
import FetchGet from '../../../shared/fetch-get';
import FetchPost from '../../../shared/fetch-post';
import { FileExtensions } from '../../../shared/file-extensions';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import Modal from '../../atoms/modal/modal';

interface IModalExportProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: Function;
  collections: boolean;
}

enum ProcessingState {
  default,
  server,
  ready,
  fail
}
/**
 * ModalExport
 * @param props input stats
 */
const ModalDownload: React.FunctionComponent<IModalExportProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDownloadSelection = language.text("Download selectie", "Download selection");
  const MessageOrginalFile = language.text("Origineel bestand", "Original file");
  const MessageOrginalFileAsZip = language.text("Origineel bestand als zip", "Original file as zip");

  const MessageThumbnailFile = "Thumbnail";
  const MessageGenericExportFail = language.text("Er is iets misgegaan met exporteren", "Something went wrong with exporting");
  const MessageExportReady = language.text("Het bestand {createZipKey} is klaar met exporteren.", "The file {createZipKey} has finished exporting.");
  const MessageDownloadAsZipArchive = language.text("Download als zip-archief", "Download as a zip archive");
  const MessageOneMomentPlease = language.text("Een moment geduld alstublieft", "One moment please");

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

    bodyParams.set("f", new URLPath().ArrayToCommaSeperatedString(props.select));
    bodyParams.set("json", "true");
    bodyParams.set("thumbnail", isThumbnail.toString());
    bodyParams.set("collections", props.collections.toString());
    setProcessing(ProcessingState.server);

    var zipKeyResult = await FetchPost(new UrlQuery().UrlExportPostZipApi(), bodyParams.toString());

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
    await ExportIntervalUpdate(zipKeyResult.data, setProcessing);
  }

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    await ExportIntervalUpdate(createZipKey, setProcessing);
  }, 3000);

  const [isDirectory, setIsDirectory] = React.useState(false);
  const [singleFileThumbnailStatus, setSingleFileThumbnailStatus] = React.useState(false);
  const [multipleCollectionPaths, setMultipleCollectionPaths] = React.useState(false);

  function next(propsSelect: string[], index: number) {
    if (index >= propsSelect.length) return;
    var selectItem = propsSelect[index];
    FetchGet(new UrlQuery().UrlIndexServerApiPath(selectItem)).then((result) => {
      if (!isDirectory) {
        setIsDirectory(result.data.pageType === PageType.Archive);
      }
      console.log(result.data.fileIndexItem.collectionPaths);

      if (!multipleCollectionPaths && result.data.fileIndexItem && result.data.fileIndexItem.collectionPaths) {
        setMultipleCollectionPaths(result.data.fileIndexItem.collectionPaths.length !== 1)
      }
      FetchGet(new UrlQuery().UrlAllowedTypesThumb(selectItem)).then((thumbResult) => {
        if (!singleFileThumbnailStatus) {
          setSingleFileThumbnailStatus(thumbResult.data)
        }

        index++;
        next(propsSelect, index)
      })
    })
  }

  useEffect(() => {
    if (!props.select) return;
    next(props.select, 0);
    // only run at startup
    // eslint-disable-next-line
  }, [])

  function PostZipOrginalFilesComponent() {
    return <button onClick={() => {
      postZip(false)
    }} className="btn btn--info" data-test="orginal">
      {MessageOrginalFileAsZip}
    </button>
  }

  function PostZipButtonsComponent() {
    return <>
      <PostZipOrginalFilesComponent />
      <button onClick={() => {
        postZip(true)
      }} className="btn btn--default" data-test="thumbnail">
        {MessageThumbnailFile}
      </button>
    </>
  }

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>

    <div className="modal content--subheader">{isProcessing !== ProcessingState.server ? MessageDownloadSelection : MessageOneMomentPlease}</div>
    <div className="modal content--text">

      {/* when selecting one file */}
      {isProcessing === ProcessingState.default && props.select && props.select.length === 1 && !isDirectory ? <>
        <a href={new UrlQuery().UrlDownloadPhotoApi(new URLPath().encodeURI(props.select[0]), false, false)} data-test="orginal"
          download={new URLPath().FileNameBreadcrumb(props.select[0])}
          target="_blank" rel="noopener noreferrer" className="btn btn--info">{MessageOrginalFile}</a>
        {singleFileThumbnailStatus ? <a href={new UrlQuery().UrlDownloadPhotoApi(new URLPath().encodeURI(props.select[0]), true, false)}
          download={new FileExtensions().GetFileNameWithoutExtension(props.select[0]) + ".jpg"} data-test="thumbnail"
          target="_blank" rel="noopener noreferrer" className={"btn btn--default"}>{MessageThumbnailFile}</a> : null}
        {multipleCollectionPaths ? <PostZipOrginalFilesComponent /> : null}
      </> : null}

      {isProcessing === ProcessingState.default && props.select && props.select.length === 1 && isDirectory ?
        <PostZipButtonsComponent /> : null}

      {isProcessing === ProcessingState.default && props.select && props.select.length >= 2 ?
        <PostZipButtonsComponent /> : null}

      {isProcessing === ProcessingState.server ? <>
        <div className="preloader preloader--inside"></div>
      </> : null}

      {isProcessing === ProcessingState.fail ? MessageGenericExportFail : null}

      {isProcessing === ProcessingState.ready ? <>
        {language.token(MessageExportReady, ["{createZipKey}"], [createZipKey])} <br />
        <a className="btn btn--default" href={new UrlQuery().UrlExportZipApi(createZipKey, false)}
          download rel="noopener noreferrer" target="_blank">
          {MessageDownloadAsZipArchive}
        </a>
      </> : null}
    </div>
  </Modal >)
};

export default ModalDownload
