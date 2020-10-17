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
  const MessageOrginalFileToken = language.text("Origineel bestand ({ext})", "Original file ({ext})");
  const MessageOrginalFilesAsZip = language.text("Originele bestanden als zip", "Original files as zip");
  const MessageOrginalFileCollectionsToken = language.text("Collectie download {ext} ", "Collection download ({ext})");

  const MessageThumbnailFile = language.text("Klein formaat (1000px)", "Thumbnail (1000px)");
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
  const [multipleCollectionExtensions, setMultipleCollectionExtensions] = React.useState([] as string[]);

  function next(propsSelect: string[], index: number) {
    if (index >= propsSelect.length) return;
    var selectItem = propsSelect[index];
    FetchGet(new UrlQuery().UrlIndexServerApiPath(selectItem)).then((result) => {
      if (!isDirectory) {
        setIsDirectory(result.data.pageType === PageType.Archive);
      }

      if (result.data.fileIndexItem && result.data.fileIndexItem.collectionPaths) console.log(result.data.fileIndexItem.collectionPaths);

      console.log(result.data.fileIndexItem);

      if (multipleCollectionExtensions.length === 0 && result.data.fileIndexItem && result.data.fileIndexItem.collectionPaths) {
        console.log('--');

        var collectionPathsExtensions: string[] = [];
        for (let index = 0; index < result.data.fileIndexItem.collectionPaths.length; index++) {
          const ext = new FileExtensions().GetFileExtensionWithoutDot(result.data.fileIndexItem.collectionPaths[index]);
          collectionPathsExtensions.push(ext)
        }
        console.log(collectionPathsExtensions);

        setMultipleCollectionExtensions(collectionPathsExtensions)
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

  type PostZipOrginalFilesComponentPropTypes = {
    content: string;
  }
  const PostZipOrginalFilesComponent: React.FunctionComponent<PostZipOrginalFilesComponentPropTypes> = (props) => {
    return <button onClick={() => {
      postZip(false)
    }} className="btn btn--info" data-test="orginal">
      {props.content}
    </button>
  };

  function PostZipButtonsComponent() {
    return <>
      <PostZipOrginalFilesComponent content={MessageOrginalFilesAsZip} />
      <button onClick={() => {
        postZip(true)
      }} className="btn btn--default" data-test="thumbnail">
        {MessageThumbnailFile}
      </button>
    </>
  }

  return (<Modal
    id="detailview-download-modal"
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
          target="_blank" rel="noopener noreferrer" className="btn btn--default">{
            language.token(MessageOrginalFileToken, ["{ext}"], [new FileExtensions().GetFileExtensionWithoutDot(props.select[0])])
          }</a>
        {singleFileThumbnailStatus ? <a href={new UrlQuery().UrlDownloadPhotoApi(new URLPath().encodeURI(props.select[0]), true, false)}
          download={new FileExtensions().GetFileNameWithoutExtension(props.select[0]) + ".jpg"} data-test="thumbnail"
          target="_blank" rel="noopener noreferrer" className={"btn btn--info"}>{MessageThumbnailFile}</a> : null}
        {multipleCollectionExtensions.length >= 2 ? <PostZipOrginalFilesComponent
          content={language.token(MessageOrginalFileCollectionsToken, ["{ext}"], [multipleCollectionExtensions.toString()])} /> : null}
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
