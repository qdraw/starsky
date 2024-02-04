import React, { useEffect } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useInterval from "../../../hooks/use-interval";
import { ExportIntervalUpdate } from "../../../shared/export/export-interval-update";
import { ProcessingState } from "../../../shared/export/processing-state";
import FetchGet from "../../../shared/fetch-get";
import FetchPost from "../../../shared/fetch-post";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";
import Select from "../../atoms/select/select";

interface IModalPublishProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: Function;
}

/**
 * ModalPublish
 * @param props input stats
 */
const ModalPublish: React.FunctionComponent<IModalPublishProps> = (props) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePublishSelection = language.text("Publiceer selectie", "Publish selection");
  const MessageGenericExportFail = language.text(
    "Er is iets misgegaan met exporteren",
    "Something went wrong with exporting"
  );
  const MessageRetryExportFail = language.text("Probeer het opnieuw", "Retry this");

  const MessageExportReady = language.text(
    "Het bestand {createZipKey} is klaar met exporteren.",
    "The file {createZipKey} has finished exporting."
  );
  const MessageDownloadAsZipArchive = language.text(
    "Download als zip-archief",
    "Download as a zip archive"
  );
  const MessageOneMomentPlease = language.text(
    "Een moment geduld alstublieft",
    "One moment please"
  );
  const MessageItemName = language.text("Waar gaat het item over?", "What is the item about?");
  const MessageItemNameInUse = language.text(
    "Deze naam is al in gebruik, kies een andere naam",
    "This name is already in use, please choose another name"
  );
  const MessagePublishProfileName = language.text("Profiel instelling", "Profile setting");

  const MessagePublishProfileNamesErrored = language.text(
    "Profiel instelling: {publishProfileNames} bevat bestand locatie fouten",
    "Profile setting: {publishProfileNames} contains filepath errors"
  );

  const [isProcessing, setIsProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");
  const [itemName, setItemName] = React.useState("");
  const [existItemName, setExistItemName] = React.useState(false);
  const [publishProfileName, setPublishProfileName] = React.useState("");

  async function postZip() {
    setExistItemName(false);

    if (!props.select) {
      setIsProcessing(ProcessingState.fail);
      return;
    }

    const bodyParams = new URLSearchParams();
    bodyParams.set("f", new URLPath().ArrayToCommaSeparatedString(props.select));
    bodyParams.set("itemName", itemName);
    bodyParams.set("publishProfileName", publishProfileName);
    bodyParams.set("force", "true");

    setIsProcessing(ProcessingState.server);

    const zipKeyResult = await FetchPost(new UrlQuery().UrlPublishCreate(), bodyParams.toString());

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setIsProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
    await ExportIntervalUpdate(zipKeyResult.data, setIsProcessing);
  }

  const allPublishProfiles = useFetch(new UrlQuery().UrlPublish(), "get").data as
    | { key: string; value: string }[]
    | null;

  useEffect(() => {
    // set the default option
    if (!allPublishProfiles) return;
    setPublishProfileName(allPublishProfiles[0].key);
  }, [allPublishProfiles]);

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    await ExportIntervalUpdate(createZipKey, setIsProcessing);
  }, 9000);

  function updateItemName(event: React.ChangeEvent<HTMLDivElement>) {
    const toUpdateItemName = event.target.textContent ? event.target.textContent.trim() : "";
    setItemName(toUpdateItemName);

    if (!toUpdateItemName) {
      setExistItemName(false);
      return;
    }

    FetchGet(new UrlQuery().UrlPublishExist(toUpdateItemName)).then((result) => {
      if (result.statusCode !== 200) return;
      setExistItemName(result.data);
    });
  }

  const existItemNameComponent = existItemName ? (
    <div className="warning-box" data-test="modal-publish-warning-box">
      {MessageItemNameInUse}
      {/* optional you could overwrite by pressing Publish*/}
    </div>
  ) : null;

  return (
    <Modal
      id="detailview-publish-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div data-test="modal-publish-subheader" className="modal content--subheader">
        {isProcessing !== ProcessingState.server ? MessagePublishSelection : MessageOneMomentPlease}
      </div>
      <div data-test="modal-publish-content-text" className="modal content--text publish">
        {/* when selecting one file */}
        {isProcessing === ProcessingState.default && props.select ? (
          <>
            <h4>{MessageItemName}</h4>

            <FormControl
              contentEditable={true}
              onInput={updateItemName}
              name="item-name"
            ></FormControl>
            {existItemNameComponent}
            <h4>{MessagePublishProfileName}</h4>
            <Select
              selectOptions={allPublishProfiles?.filter((x) => x.value).map((x) => x.key)}
              callback={setPublishProfileName}
            ></Select>

            {allPublishProfiles?.filter((x) => !x.value)?.length ? (
              <div
                className="warning-box warning-box--optional"
                data-test="publish-profile-preflight-error"
              >
                {language.token(
                  MessagePublishProfileNamesErrored,
                  ["{publishProfileNames}"],
                  [
                    allPublishProfiles
                      .filter((x) => !x.value)
                      .map((x) => x.key)
                      .join(", ")
                  ]
                )}
                <br />
              </div>
            ) : null}

            <button
              disabled={!itemName || !publishProfileName}
              onClick={postZip}
              className="btn btn--default"
              data-test="publish"
            >
              {MessagePublishSelection}
            </button>
          </>
        ) : null}

        {isProcessing === ProcessingState.server ? (
          <div className="preloader preloader--inside"></div>
        ) : null}

        {isProcessing === ProcessingState.fail ? (
          <>
            {MessageGenericExportFail} <br />
            <button
              onClick={() => setIsProcessing(ProcessingState.default)}
              className="btn btn--info"
              data-test="publish-retry-export-fail"
            >
              {MessageRetryExportFail}
            </button>
          </>
        ) : null}

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

export default ModalPublish;
