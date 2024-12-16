import React, { useEffect } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useInterval from "../../../hooks/use-interval";
import localization from "../../../localization/localization.json";
import { ExportIntervalUpdate } from "../../../shared/export/export-interval-update";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalExportProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: () => void;
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
  const MessageDownloadSelection = language.key(localization.MessageDownloadSelection);
  const MessageOriginalFile = language.key(localization.MessageOriginalFile);
  const MessageThumbnailFile = language.key(localization.MessageThumbnailFile);
  const MessageGenericExportFail = language.key(localization.MessageGenericExportFail);
  const MessageExportReady = language.key(localization.MessageExportReady);
  const MessageDownloadAsZipArchive = language.key(localization.MessageDownloadAsZipArchive);
  const MessageOneMomentPlease = language.key(localization.MessageOneMomentPlease);

  const [isProcessing, setIsProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");

  async function postZip(isThumbnail: boolean) {
    if (!props.select) {
      setIsProcessing(ProcessingState.fail);
      return;
    }
    /*
    f	/__starsky/0001-readonly/4.jpg;/__starsky/0001-readonly/3.jpg
    json	true
    thumbnail	false
    collections	true
    */
    const bodyParams = new URLSearchParams();

    bodyParams.set("f", new URLPath().ArrayToCommaSeparatedString(props.select));
    bodyParams.set("json", "true");
    bodyParams.set("thumbnail", isThumbnail.toString());
    bodyParams.set("collections", props.collections.toString());
    setIsProcessing(ProcessingState.server);

    const zipKeyResult = await FetchPost(
      new UrlQuery().UrlExportPostZipApi(),
      bodyParams.toString()
    );

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setIsProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
    await ExportIntervalUpdate(zipKeyResult.data, setIsProcessing);
  }

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    await ExportIntervalUpdate(createZipKey, setIsProcessing);
  }, 3000);

  const [singleFileThumbnailStatus, setSingleFileThumbnailStatus] = React.useState(true);

  function getFirstSelectResult(): string {
    if (!props.select || props.select.length !== 1) return "";
    return props.select[0];
  }

  const singleFileThumbResult = useFetch(
    new UrlQuery().UrlAllowedTypesThumb(getFirstSelectResult()),
    "get"
  );
  useEffect(() => {
    setSingleFileThumbnailStatus(singleFileThumbResult.data !== false);
  }, [singleFileThumbResult.data]);

  return (
    <Modal
      id="detailview-export-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="modal content--subheader">
        {isProcessing !== ProcessingState.server
          ? MessageDownloadSelection
          : MessageOneMomentPlease}
      </div>
      <div className="modal content--text">
        {/* when selecting one file */}
        {isProcessing === ProcessingState.default && props.select && props.select.length === 1 ? (
          <>
            <a
              href={new UrlQuery().UrlDownloadPhotoApi(
                new URLPath().encodeURI(props.select[0]),
                false,
                false
              )}
              data-test="original"
              download={new URLPath().FileNameBreadcrumb(props.select[0])}
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn--info"
            >
              {MessageOriginalFile}
            </a>
            {singleFileThumbnailStatus ? (
              <a
                href={new UrlQuery().UrlDownloadPhotoApi(
                  new URLPath().encodeURI(props.select[0]),
                  true,
                  false
                )}
                download={
                  new FileExtensions().GetFileNameWithoutExtension(props.select[0]) + ".jpg"
                }
                data-test="thumbnail"
                target="_blank"
                rel="noopener noreferrer"
                className={"btn btn--default"}
              >
                {MessageThumbnailFile}
              </a>
            ) : null}
          </>
        ) : null}

        {isProcessing === ProcessingState.default && props.select && props.select.length >= 2 ? (
          <>
            <button
              onClick={() => {
                postZip(false);
              }}
              className="btn btn--info"
              data-test="original"
            >
              {MessageOriginalFile}
            </button>

            <button
              onClick={() => {
                postZip(true);
              }}
              className="btn btn--default"
              data-test="thumbnail"
            >
              {MessageThumbnailFile}
            </button>
          </>
        ) : null}

        {isProcessing === ProcessingState.server ? (
          <div className="preloader preloader--inside"></div>
        ) : null}

        {isProcessing === ProcessingState.fail ? MessageGenericExportFail : null}

        {isProcessing === ProcessingState.ready ? (
          <>
            {language.token(MessageExportReady, ["{createZipKey}"], [createZipKey])} <br />
            <a
              className="btn btn--default"
              href={new UrlQuery().UrlExportZipApi(createZipKey, false)}
              download
              rel="noopener noreferrer"
              target="_blank"
            >
              {MessageDownloadAsZipArchive}
            </a>
          </>
        ) : null}
      </div>
    </Modal>
  );
};

export default ModalDownload;
